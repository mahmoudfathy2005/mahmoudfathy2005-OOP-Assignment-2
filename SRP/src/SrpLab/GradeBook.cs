
namespace SrpLab;


/// <summary>
/// Coordinates student scores, grading policy, and transcript output.
/// </summary>
public sealed class GradeBook
{
    private readonly StudentScores _scores = new();
    private readonly GradePolicy _policy = new();
    private readonly TranscriptFormatter _formatter = new();

    public void Record(string studentId, decimal score)
    {
        _scores.Record(studentId, score);
    }

    public decimal Average(string studentId)
    {
        return _scores.Average(studentId);
    }

    public string Letter(string studentId)
    {
        return _policy.Letter(Average(studentId));
    }

    public bool MeetsHonorRoll(string studentId)
    {
        return _policy.MeetsHonorRoll(Average(studentId));
    }

    public string TranscriptPlain(string studentId, string fullName)
    {
        return _formatter.TranscriptPlain(
            studentId,
            fullName,
            Average(studentId),
            Letter(studentId),
            MeetsHonorRoll(studentId));
    }

    public string ExportCsv()
    {
        var rows = new List<string>
        {
            "studentId,average,letter,honor"
        };

        foreach (var id in _scores.StudentIds.OrderBy(x => x))
        {
            rows.Add(
                $"{id},{Average(id)},{Letter(id)}," +
                $"{(MeetsHonorRoll(id) ? 1 : 0)}");
        }

        return _formatter.ExportCsv(rows);
    }
}


public sealed class StudentScores
{
    private readonly Dictionary<string, List<decimal>> _scores =
        new(StringComparer.OrdinalIgnoreCase);

    public void Record(string studentId, decimal score)
    {
        if (score is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(score));

        if (!_scores.TryGetValue(studentId, out var list))
        {
            list = new List<decimal>();
            _scores[studentId] = list;
        }

        list.Add(score);
    }

    public decimal Average(string studentId)
    {
        if (!_scores.TryGetValue(studentId, out var list) ||
            list.Count == 0)
        {
            return 0m;
        }

        return Math.Round(list.Average(), 2);
    }

    public IEnumerable<string> StudentIds => _scores.Keys;
}




public sealed class GradePolicy
{
    public string Letter(decimal average)
    {
        if (average >= 90) return "A";
        if (average >= 80) return "B";
        if (average >= 70) return "C";
        if (average >= 60) return "D";
        return "F";
    }

    public bool MeetsHonorRoll(decimal average)
    {
        return average >= 85 &&
               (Letter(average) is "A" or "B");
    }
}





public sealed class TranscriptFormatter
{
    public string TranscriptPlain(
        string studentId,
        string fullName,
        decimal average,
        string letter,
        bool honor)
    {
        return $"TRANSCRIPT\nStudent: {fullName} ({studentId})\n" +
               $"Average: {average}\nLetter: {letter}\n" +
               $"Honor: {honor}\n";
    }

    public string ExportCsv(IEnumerable<string> rows)
    {
        return string.Join('\n', rows);
    }
}
