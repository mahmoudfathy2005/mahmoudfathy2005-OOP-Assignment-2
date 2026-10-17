
namespace SrpLab;




public sealed class AppointmentDesk
{
    private readonly AppointmentSchedule _schedule;
    private readonly AppointmentBooking _booking;
    private readonly IcsCalendarGenerator _icsGenerator;
    private readonly SmsReminderFormatter _smsFormatter;

    public TimeOnly Open => _schedule.Open;
    public TimeOnly Close => _schedule.Close;
    public int SlotMinutes => _schedule.SlotMinutes;

    public AppointmentDesk(
        TimeOnly open,
        TimeOnly close,
        int slotMinutes)
    {
        _schedule = new AppointmentSchedule(
            open, close, slotMinutes);

        _booking = new AppointmentBooking(_schedule);
        _icsGenerator = new IcsCalendarGenerator();
        _smsFormatter = new SmsReminderFormatter();
    }

    public bool IsWithinBusinessHours(DateTimeOffset when)
    {
        return _schedule.IsWithinBusinessHours(when);
    }

    public DateTimeOffset? FindNextSlot(
        DateTimeOffset from,
        int searchHours)
    {
        return _booking.FindNextSlot(from, searchHours);
    }

    public bool TryBook(DateTimeOffset slot)
    {
        return _booking.TryBook(slot);
    }

    public string ToIcs(
        DateTimeOffset slot,
        string patientName,
        string clinician)
    {
        return _icsGenerator.Generate(
            slot, SlotMinutes, patientName, clinician);
    }

    public string SmsReminder(
        DateTimeOffset slot,
        string clinicPhone)
    {
        return _smsFormatter.Format(slot, clinicPhone);
    }
}


public sealed class AppointmentSchedule
{
    public TimeOnly Open { get; }
    public TimeOnly Close { get; }
    public int SlotMinutes { get; }

    public AppointmentSchedule(
        TimeOnly open,
        TimeOnly close,
        int slotMinutes)
    {
        Open = open;
        Close = close;
        SlotMinutes = slotMinutes;
    }

    public bool IsWithinBusinessHours(DateTimeOffset when)
    {
        if (when.DayOfWeek is DayOfWeek.Friday
            or DayOfWeek.Saturday)
        {
            return false;
        }

        var time = TimeOnly.FromDateTime(when.DateTime);

        return time >= Open
            && time.AddMinutes(SlotMinutes) <= Close;
    }

    public DateTimeOffset Align(DateTimeOffset from)
    {
        var minutes =
            from.Minute - (from.Minute % SlotMinutes);

        return new DateTimeOffset(
            from.Year,
            from.Month,
            from.Day,
            from.Hour,
            minutes,
            0,
            from.Offset);
    }
}




public sealed class AppointmentBooking
{
    private readonly HashSet<DateTimeOffset> _booked = new();
    private readonly AppointmentSchedule _schedule;

    public AppointmentBooking(AppointmentSchedule schedule)
    {
        _schedule = schedule;
    }

    public DateTimeOffset? FindNextSlot(
        DateTimeOffset from,
        int searchHours)
    {
        var cursor = _schedule.Align(from);
        var end = from.AddHours(searchHours);

        while (cursor < end)
        {
            if (_schedule.IsWithinBusinessHours(cursor)
                && !_booked.Contains(cursor))
            {
                return cursor;
            }

            cursor = cursor.AddMinutes(
                _schedule.SlotMinutes);
        }

        return null;
    }

    public bool TryBook(DateTimeOffset slot)
    {
        if (!_schedule.IsWithinBusinessHours(slot)
            || _booked.Contains(slot))
        {
            return false;
        }

        _booked.Add(slot);
        return true;
    }
}





public sealed class IcsCalendarGenerator
{
    public string Generate(
        DateTimeOffset slot,
        int slotMinutes,
        string patientName,
        string clinician)
    {
        var uid = Guid.NewGuid();
        var end = slot.AddMinutes(slotMinutes);

        return "BEGIN:VCALENDAR\nVERSION:2.0\nBEGIN:VEVENT\n" +
               $"UID:{uid}\nDTSTART:{slot:yyyyMMdd'T'HHmmss'Z'}\nDTEND:{end:yyyyMMdd'T'HHmmss'Z'}\n" +
               $"SUMMARY:Visit {patientName} / {clinician}\nEND:VEVENT\nEND:VCALENDAR\n";
    }
}






public sealed class SmsReminderFormatter
{
    public string Format(
        DateTimeOffset slot,
        string clinicPhone)
    {
        return $"Reminder: appointment {slot:MMM dd HH:mm}. " +
               $"Call {clinicPhone} to reschedule.";
    }
}
