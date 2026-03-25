using System.Text;
using CandidacyManagement.Domain.Entities;

namespace CandidacyManagement.Application.Calendar;

/// <summary>
/// מימוש שירות יומן אלקטרוני - יצירת תוכן iCal בפורמט RFC 5545.
/// בשלב זה השירות מייצר את מחרוזת ה-iCal בלבד.
/// שליחת הדוא"ל בפועל תתבצע במודול ההתראות (משימה 13).
/// </summary>
public class CalendarService : ICalendarService
{
    public Task<string> SendInterviewInviteAsync(Interview interview, List<int> interviewerIds, CancellationToken cancellationToken = default)
    {
        var icalContent = GenerateICalEvent(interview, interviewerIds, "REQUEST");
        return Task.FromResult(icalContent);
    }

    public Task<string> CancelInterviewInviteAsync(int interviewId, CancellationToken cancellationToken = default)
    {
        var icalContent = GenerateCancelEvent(interviewId);
        return Task.FromResult(icalContent);
    }

    private static string GenerateICalEvent(Interview interview, List<int> interviewerIds, string method)
    {
        var uid = $"interview-{interview.Id}@candidacy-management";
        var dtStart = interview.ScheduledDate.Date.Add(interview.StartTime);
        var dtEnd = interview.ScheduledDate.Date.Add(interview.EndTime);

        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//CandidacyManagement//Interview//HE");
        sb.AppendLine($"METHOD:{method}");
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"UID:{uid}");
        sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
        sb.AppendLine($"DTSTART:{dtStart:yyyyMMddTHHmmssZ}");
        sb.AppendLine($"DTEND:{dtEnd:yyyyMMddTHHmmssZ}");
        sb.AppendLine($"SUMMARY:ראיון מועמדות #{interview.CandidacyId}");
        sb.AppendLine($"LOCATION:{interview.Location ?? string.Empty}");
        sb.AppendLine($"DESCRIPTION:ראיון {(interview.InterviewType == Domain.Enums.InterviewType.Second ? "שני" : "ראשון")} - מועמדות #{interview.CandidacyId}");
        sb.AppendLine($"ORGANIZER:mailto:system@candidacy-management.gov.il");
        sb.AppendLine($"STATUS:CONFIRMED");
        sb.AppendLine($"SEQUENCE:0");

        foreach (var interviewerId in interviewerIds)
        {
            sb.AppendLine($"ATTENDEE;ROLE=REQ-PARTICIPANT;PARTSTAT=NEEDS-ACTION:mailto:interviewer-{interviewerId}@candidacy-management.gov.il");
        }

        sb.AppendLine("END:VEVENT");
        sb.AppendLine("END:VCALENDAR");

        return sb.ToString();
    }

    private static string GenerateCancelEvent(int interviewId)
    {
        var uid = $"interview-{interviewId}@candidacy-management";

        var sb = new StringBuilder();
        sb.AppendLine("BEGIN:VCALENDAR");
        sb.AppendLine("VERSION:2.0");
        sb.AppendLine("PRODID:-//CandidacyManagement//Interview//HE");
        sb.AppendLine("METHOD:CANCEL");
        sb.AppendLine("BEGIN:VEVENT");
        sb.AppendLine($"UID:{uid}");
        sb.AppendLine($"DTSTAMP:{DateTime.UtcNow:yyyyMMddTHHmmssZ}");
        sb.AppendLine("STATUS:CANCELLED");
        sb.AppendLine("SEQUENCE:1");
        sb.AppendLine("END:VEVENT");
        sb.AppendLine("END:VCALENDAR");

        return sb.ToString();
    }
}
