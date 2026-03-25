using CandidacyManagement.Domain.Entities;

namespace CandidacyManagement.Application.Calendar;

/// <summary>
/// שירות יומן אלקטרוני - יצירת אירועי iCal לראיונות
/// </summary>
public interface ICalendarService
{
    /// <summary>
    /// יצירת אירוע יומן (iCal) עבור ראיון ושליחתו למראיינים
    /// </summary>
    Task<string> SendInterviewInviteAsync(Interview interview, List<int> interviewerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// ביטול אירוע יומן עבור ראיון
    /// </summary>
    Task<string> CancelInterviewInviteAsync(int interviewId, CancellationToken cancellationToken = default);
}
