namespace PatternsLab.Problems.Builder;

public sealed class CourseRegistration
{
    public string StudentEmail { get; }
    public string CourseCode { get; }
    public string AccessMode { get; }
    public string? GroupCode { get; }
    public string? DiscountCode { get; }
    public bool SendWhatsApp { get; }
    public bool SendEmailWelcome { get; }
    public string? MentorNote { get; }
    public DateOnly? PreferredStart { get; }

    public CourseRegistration(
        string studentEmail,
        string courseCode,
        string accessMode,
        string? groupCode,
        string? discountCode,
        bool sendWhatsApp,
        bool sendEmailWelcome,
        string? mentorNote,
        DateOnly? preferredStart)
    {
        if (string.IsNullOrWhiteSpace(studentEmail)) throw new ArgumentException("email required");
        if (string.IsNullOrWhiteSpace(courseCode)) throw new ArgumentException("course required");

        if (accessMode == "LiveGroup" && string.IsNullOrWhiteSpace(groupCode))
            throw new InvalidOperationException("LiveGroup requires GroupCode");
        if (accessMode == "VideosOnly" && !string.IsNullOrWhiteSpace(groupCode))
            throw new InvalidOperationException("VideosOnly cannot have GroupCode");

        StudentEmail = studentEmail;
        CourseCode = courseCode;
        AccessMode = accessMode;
        GroupCode = groupCode;
        DiscountCode = discountCode;
        SendWhatsApp = sendWhatsApp;
        SendEmailWelcome = sendEmailWelcome;
        MentorNote = mentorNote;
        PreferredStart = preferredStart;
    }

    public override string ToString()
        => $"{StudentEmail} → {CourseCode} [{AccessMode}] group={GroupCode ?? "-"} discount={DiscountCode ?? "-"} wa={SendWhatsApp} mail={SendEmailWelcome}";
}





public sealed class CourseRegistrationBuilder
{
    private string? _studentEmail;
    private string? _courseCode;


    private string? _accessMode;
    private string? _groupCode;

    private string? _discountCode;
    private bool _sendWhatsApp;
    private bool _sendEmailWelcome;
    private string? _mentorNote;
    private DateOnly? _preferredStart;

    public CourseRegistrationBuilder ForStudent(string email)
    {
        _studentEmail = email;
        return this;
    }

    public CourseRegistrationBuilder ForCourse(string courseCode)
    {
        _courseCode = courseCode;
        return this;
    }

    public CourseRegistrationBuilder WithLiveGroup(string? groupCode)
    {
        _accessMode = "LiveGroup";
        _groupCode = groupCode;
        return this;
    }

    public CourseRegistrationBuilder WithVideosOnly()
    {
        _accessMode = "VideosOnly";
        _groupCode = null;
        return this;
    }

    public CourseRegistrationBuilder WithDiscount(string? discountCode)
    {
        _discountCode = discountCode;
        return this;
    }

    public CourseRegistrationBuilder SendWhatsApp()
    {
        _sendWhatsApp = true;
        return this;
    }

    public CourseRegistrationBuilder SendEmailWelcome()
    {
        _sendEmailWelcome = true;
        return this;
    }

    public CourseRegistrationBuilder WithMentorNote(string? note)
    {
        _mentorNote = note;
        return this;
    }

    public CourseRegistrationBuilder StartingOn(DateOnly? date)
    {
        _preferredStart = date;
        return this;
    }

    public CourseRegistration Build()
    {
        if (string.IsNullOrWhiteSpace(_studentEmail))
        {
            throw new InvalidOperationException(
                "StudentEmail is required.");
        }

        if (string.IsNullOrWhiteSpace(_courseCode))
        {
            throw new InvalidOperationException(
                "CourseCode is required.");
        }

        if (_accessMode is not ("LiveGroup" or "VideosOnly"))
        {
            throw new InvalidOperationException(
                "AccessMode must be selected using WithLiveGroup() " +
                "or WithVideosOnly().");
        }

        if (_accessMode == "LiveGroup" &&
            string.IsNullOrWhiteSpace(_groupCode))
        {
            throw new InvalidOperationException(
                "LiveGroup requires a non-empty GroupCode.");
        }

        if (_accessMode == "VideosOnly" &&
            !string.IsNullOrWhiteSpace(_groupCode))
        {
            throw new InvalidOperationException(
                "VideosOnly cannot have a GroupCode.");
        }

        return new CourseRegistration(
            _studentEmail,
            _courseCode,
            _accessMode,
            _groupCode,
            _discountCode,
            _sendWhatsApp,
            _sendEmailWelcome,
            _mentorNote,
            _preferredStart
        );
    }


}


public static class RegistrationCallSites
{
    public static CourseRegistration CreateLiveStudentUgly()
    {
        return new CourseRegistrationBuilder()
        .ForStudent("[sara@mail.com](mailto:sara@mail.com)")
        .ForCourse("SEF-101")
        .WithLiveGroup("G1")
        .WithDiscount("EARLY10")
        .SendWhatsApp()
        .SendEmailWelcome()
        .WithMentorNote("Needs evening slot")
        .StartingOn(new DateOnly(2026, 10, 1))
        .Build();
    }


public static CourseRegistration CreateVideosOnlyUgly()
    {
        return new CourseRegistrationBuilder()
            .ForStudent("ali@mail.com")
            .ForCourse("SEF-101")
            .WithVideosOnly()
            .SendEmailWelcome()
            .Build();
    }


}

