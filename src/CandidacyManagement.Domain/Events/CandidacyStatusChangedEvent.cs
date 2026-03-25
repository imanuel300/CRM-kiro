using MediatR;

namespace CandidacyManagement.Domain.Events;

/// <summary>
/// אירוע דומיין - שינוי סטטוס מועמדות
/// </summary>
public record CandidacyStatusChangedEvent(
    int CandidacyId,
    int OrgUnitId,
    string FromStatusCode,
    string ToStatusCode,
    int ChangedByUserId) : INotification;
