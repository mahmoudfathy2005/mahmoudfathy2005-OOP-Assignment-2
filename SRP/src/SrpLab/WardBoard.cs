

namespace SrpLab;

/// <summary>
/// Coordinates ward bed tracking, acuity scoring, paging, and output.
/// </summary>
public sealed class WardBoard
{
    private readonly WardBedRegistry _registry = new();
    private readonly AcuityScorer _acuityScorer = new();
    private readonly WardPager _pager = new();
    private readonly HandoffNoteFormatter _handoffFormatter = new();
    private readonly WardCensusExporter _censusExporter = new();

    public void AssignBed(int bed, string patientId, int heartRate, int spo2)
    {
        if (bed <= 0)
            throw new ArgumentOutOfRangeException(nameof(bed));

        if (string.IsNullOrWhiteSpace(patientId))
            throw new ArgumentException("patient required");

        var normalizedPatientId = patientId.Trim().ToUpperInvariant();
        var acuity = ScoreAcuity(heartRate, spo2);

        _registry.Assign(bed, normalizedPatientId, acuity);
        _pager.CheckAndLog(bed, acuity);
    }

    public int ScoreAcuity(int heartRate, int spo2)
    {
        return _acuityScorer.Score(heartRate, spo2);
    }

    public string BuildHandoffNote(int bed)
    {
        if (!_registry.TryGetPatient(bed, out var patient))
            return $"Bed {bed}: empty";

        var acuity = _registry.GetAcuity(bed);

        return _handoffFormatter.Format(bed, patient, acuity);
    }

    public IReadOnlyList<string> DrainPagerLog()
    {
        return _pager.DrainLog();
    }

    public string ExportCensusCsv()
    {
        return _censusExporter.Export(_registry);
    }
}


public sealed class AcuityScorer
{
    public int Score(int heartRate, int spo2)
    {
        var score = 0;

        if (heartRate > 120 || heartRate < 45)
            score += 4;
        else if (heartRate > 100)
            score += 2;

        if (spo2 < 90)
            score += 5;
        else if (spo2 < 94)
            score += 2;

        return Math.Min(score, 10);
    }
}



public sealed class WardBedRegistry
{
    private readonly Dictionary<int, string> _bedPatient = new();
    private readonly Dictionary<int, int> _vitalsScore = new();

    public void Assign(int bed, string patientId, int acuity)
    {
        _bedPatient[bed] = patientId;
        _vitalsScore[bed] = acuity;
    }

    public bool TryGetPatient(int bed, out string patientId)
    {
        return _bedPatient.TryGetValue(bed, out patientId!);
    }

    public int GetAcuity(int bed)
    {
        return _vitalsScore[bed];
    }

    public IEnumerable<int> GetBeds()
    {
        return _bedPatient.Keys.OrderBy(bed => bed);
    }

    public string GetPatient(int bed)
    {
        return _bedPatient[bed];
    }
}



public sealed class WardPager
{
    private readonly List<string> _pagerLog = new();

    public void CheckAndLog(int bed, int acuity)
    {
        if (acuity >= 8)
            _pagerLog.Add($"CODE-YELLOW bed={bed} at {DateTime.UtcNow:HH:mm}");
    }

    public IReadOnlyList<string> DrainLog()
    {
        var copy = _pagerLog.ToList();
        _pagerLog.Clear();
        return copy;
    }
}



public sealed class HandoffNoteFormatter
{
    public string Format(int bed, string patient, int acuity)
    {
        var tone = acuity >= 8
            ? "ESCALATE"
            : acuity >= 4
                ? "WATCH"
                : "STABLE";

        return $"[HANDOFF {DateTime.UtcNow:yyyy-MM-dd}] Bed {bed} · {patient} · acuity={acuity} · {tone}";
    }
}



public sealed class WardCensusExporter
{
    public string Export(WardBedRegistry registry)
    {
        var lines = new List<string>
        {
            "bed,patient,acuity"
        };

        foreach (var bed in registry.GetBeds())
        {
            lines.Add(
                $"{bed},{registry.GetPatient(bed)},{registry.GetAcuity(bed)}");
        }

        return string.Join('\n', lines);
    }
}
