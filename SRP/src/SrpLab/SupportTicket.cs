

namespace SrpLab;

/// <summary>
/// Coordinates support ticket data, priority analysis, SLA, and responses.
/// </summary>
public sealed class SupportTicket
{
    private readonly TicketPriorityAnalyzer _priorityAnalyzer = new();
    private readonly TicketSlaCalculator _slaCalculator = new();
    private readonly PublicReplyFormatter _replyFormatter = new();
    private readonly TicketEscalationFormatter _escalationFormatter = new();

    public string Id { get; }
    public string Subject { get; private set; }
    public string Body { get; private set; }
    public DateTimeOffset OpenedAt { get; }
    public string Priority { get; private set; } = "P3";

    public SupportTicket(
        string id,
        string subject,
        string body,
        DateTimeOffset openedAt)
    {
        Id = id;
        Subject = subject;
        Body = body;
        OpenedAt = openedAt;

        RecalculatePriorityFromText();
    }

    public void AppendCustomerMessage(string text)
    {
        Body += "\n---\n" + text;
        RecalculatePriorityFromText();
    }

    public void RecalculatePriorityFromText()
    {
        Priority = _priorityAnalyzer.Analyze(Subject, Body);
    }

    public DateTimeOffset SlaDeadline()
    {
        return _slaCalculator.CalculateDeadline(OpenedAt, Priority);
    }

    public bool IsBreached(DateTimeOffset now)
    {
        return _slaCalculator.IsBreached(SlaDeadline(), now);
    }

    public string DraftPublicReply(string agentName)
    {
        return _replyFormatter.Format(
            Id,
            agentName,
            Priority,
            SlaDeadline());
    }

    public string InternalEscalationBlurb()
    {
        return _escalationFormatter.Format(
            Id,
            Priority,
            SlaDeadline());
    }
}


public sealed class TicketPriorityAnalyzer
{
    public string Analyze(string subject, string body)
    {
        var blob = (subject + " " + body).ToLowerInvariant();

        if (blob.Contains("down") ||
            blob.Contains("outage") ||
            blob.Contains("cannot login"))
        {
            return "P1";
        }

        if (blob.Contains("urgent") ||
            blob.Contains("asap") ||
            blob.Contains("blocked"))
        {
            return "P2";
        }

        return "P3";
    }
}



public sealed class TicketSlaCalculator
{
    public DateTimeOffset CalculateDeadline(
        DateTimeOffset openedAt,
        string priority)
    {
        var hours = priority switch
        {
            "P1" => 4,
            "P2" => 24,
            _ => 72
        };

        return openedAt.AddHours(hours);
    }

    public bool IsBreached(DateTimeOffset deadline, DateTimeOffset now)
    {
        return now > deadline;
    }
}


public sealed class PublicReplyFormatter
{
    public string Format(
        string ticketId,
        string agentName,
        string priority,
        DateTimeOffset slaDeadline)
    {
        var apology = priority == "P1"
            ? "We are treating this as a critical incident."
            : "Thanks for reaching out.";

        return $"Hi,\n{apology}\nTicket {ticketId} is with {agentName}. Next update before {slaDeadline:u}.\n";
    }
}




public sealed class TicketEscalationFormatter
{
    public string Format(
        string ticketId,
        string priority,
        DateTimeOffset slaDeadline)
    {
        return $"ESCALATE {ticketId} priority={priority} breachAt={slaDeadline:u} keywords-scanned=yes";
    }
}
