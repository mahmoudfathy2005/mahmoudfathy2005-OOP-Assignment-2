

namespace SrpLab;

/// <summary>
/// Coordinates course enrollment operations.
/// </summary>
public sealed class CourseEnrollmentDesk
{
    private readonly EnrollmentRegistry _registry = new();
    private readonly EnrollmentWaitlist _waitlist = new();
    private readonly WelcomePacketFormatter _welcomeFormatter = new();
    private readonly TuitionInvoiceFormatter _invoiceFormatter = new();

    public int Capacity { get; }
    public decimal Tuition { get; }
    public string CourseCode { get; }

    public CourseEnrollmentDesk(
        string courseCode,
        int capacity,
        decimal tuition)
    {
        CourseCode = courseCode;
        Capacity = capacity;
        Tuition = tuition;
    }

    public string Register(string studentEmail)
    {
        if (string.IsNullOrWhiteSpace(studentEmail))
            throw new ArgumentException("email");

        var email = studentEmail.Trim();

        if (_registry.Contains(email) || _waitlist.Contains(email))
            return "ALREADY_REGISTERED";

        if (_registry.HasCapacity(Capacity))
        {
            _registry.Add(email);
            return "SEATED";
        }

        _waitlist.Add(email);
        return $"WAITLIST:{_waitlist.Count}";
    }

    public int WaitlistPosition(string studentEmail)
    {
        return _waitlist.Position(studentEmail);
    }

    public string WelcomePacketMarkdown(
        string studentEmail,
        string studentName)
    {
        var isSeated = _registry.Contains(studentEmail);

        var position = isSeated
            ? 0
            : WaitlistPosition(studentEmail);

        return _welcomeFormatter.Format(
            CourseCode,
            studentName,
            isSeated,
            position);
    }

    public string TuitionInvoiceLine(string studentEmail)
    {
        return _invoiceFormatter.Format(
            CourseCode,
            Tuition,
            _registry.Contains(studentEmail));
    }

    public void PromoteFromWaitlist(int seats)
    {
        _waitlist.Promote(seats, _registry, Capacity);
    }
}


public sealed class EnrollmentRegistry
{
    private readonly HashSet<string> _seated =
        new(StringComparer.OrdinalIgnoreCase);

    public int Count => _seated.Count;

    public bool Contains(string studentEmail)
    {
        return _seated.Contains(studentEmail);
    }

    public bool HasCapacity(int capacity)
    {
        return _seated.Count < capacity;
    }

    public void Add(string studentEmail)
    {
        _seated.Add(studentEmail);
    }
}





public sealed class EnrollmentWaitlist
{
    private readonly List<string> _students = new();

    public int Count => _students.Count;

    public bool Contains(string studentEmail)
    {
        return _students.Contains(studentEmail);
    }

    public void Add(string studentEmail)
    {
        _students.Add(studentEmail);
    }

    public int Position(string studentEmail)
    {
        var index = _students.FindIndex(
            email => email.Equals(
                studentEmail,
                StringComparison.OrdinalIgnoreCase));

        return index < 0 ? -1 : index + 1;
    }

    public void Promote(
        int seats,
        EnrollmentRegistry registry,
        int capacity)
    {
        while (seats > 0
            && _students.Count > 0
            && registry.HasCapacity(capacity))
        {
            var next = _students[0];

            _students.RemoveAt(0);
            registry.Add(next);

            seats--;
        }
    }
}



public sealed class WelcomePacketFormatter
{
    public string Format(
        string courseCode,
        string studentName,
        bool isSeated,
        int waitlistPosition)
    {
        var status = isSeated
            ? "confirmed seat"
            : $"waitlist #{waitlistPosition}";

        return $"# Welcome to {courseCode}\nHi {studentName},\n" +
               $"Your status: **{status}**.\n" +
               $"Bring a laptop. Discord onboarding link: " +
               $"https://example.invalid/{courseCode.ToLowerInvariant()}\n";
    }
}




public sealed class TuitionInvoiceFormatter
{
    public string Format(
        string courseCode,
        decimal tuition,
        bool isSeated)
    {
        if (!isSeated)
            return $"{courseCode},WAITLIST,0.00";

        var vat = Math.Round(tuition * 0.14m, 2);

        return $"{courseCode},TUITION,{tuition:0.00}," +
               $"VAT,{vat:0.00},TOTAL,{(tuition + vat):0.00}";
    }
}

