using System.Text.Json;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Committees;

public class CommitteeService : ICommitteeService
{
    private readonly IRepository<Committee> _committeeRepository;
    private readonly IRepository<CommitteeMeeting> _meetingRepository;
    private readonly IRepository<CommitteeDecision> _decisionRepository;
    private readonly IRepository<CommitteeAppeal> _appealRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IRepository<StatusDefinition> _statusRepository;
    private readonly IRepository<StatusTransition> _transitionRepository;
    private readonly IRepository<CandidacyStatusHistory> _historyRepository;

    public CommitteeService(
        IRepository<Committee> committeeRepository,
        IRepository<CommitteeMeeting> meetingRepository,
        IRepository<CommitteeDecision> decisionRepository,
        IRepository<CommitteeAppeal> appealRepository,
        IRepository<Candidacy> candidacyRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IRepository<StatusDefinition> statusRepository,
        IRepository<StatusTransition> transitionRepository,
        IRepository<CandidacyStatusHistory> historyRepository)
    {
        _committeeRepository = committeeRepository;
        _meetingRepository = meetingRepository;
        _decisionRepository = decisionRepository;
        _appealRepository = appealRepository;
        _candidacyRepository = candidacyRepository;
        _orgUnitRepository = orgUnitRepository;
        _statusRepository = statusRepository;
        _transitionRepository = transitionRepository;
        _historyRepository = historyRepository;
    }

    // --- Committee CRUD ---

    public async Task<CommitteeDto> CreateAsync(CreateCommitteeCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "יש לציין שם ועדה");

        if (command.Members == null || command.Members.Count == 0)
            throw new ValidationException("Members", "יש לציין לפחות חבר ועדה אחד");

        var entity = new Committee
        {
            OrgUnitId = command.OrgUnitId,
            Name = command.Name,
            Description = command.Description,
            MembersJson = JsonSerializer.Serialize(command.Members)
        };

        await _committeeRepository.AddAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<CommitteeDto> UpdateAsync(UpdateCommitteeCommand command, CancellationToken cancellationToken = default)
    {
        var entity = await _committeeRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Committee), command.Id);

        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "יש לציין שם ועדה");

        if (command.Members == null || command.Members.Count == 0)
            throw new ValidationException("Members", "יש לציין לפחות חבר ועדה אחד");

        entity.Name = command.Name;
        entity.Description = command.Description;
        entity.MembersJson = JsonSerializer.Serialize(command.Members);

        await _committeeRepository.UpdateAsync(entity, cancellationToken);
        return ToDto(entity);
    }

    public async Task<CommitteeDto> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _committeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Committee), id);

        return ToDto(entity);
    }

    public async Task<IEnumerable<CommitteeDto>> ListAsync(CommitteeQueryParams query, CancellationToken cancellationToken = default)
    {
        var results = await _committeeRepository.FindAsync(c =>
            !query.OrgUnitId.HasValue || c.OrgUnitId == query.OrgUnitId.Value,
            cancellationToken);

        return results.Select(ToDto);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _committeeRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Committee), id);

        await _committeeRepository.DeleteAsync(entity, cancellationToken);
    }

    // --- Meetings ---

    public async Task<CommitteeMeetingDto> CreateMeetingAsync(CreateMeetingCommand command, CancellationToken cancellationToken = default)
    {
        _ = await _committeeRepository.GetByIdAsync(command.CommitteeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Committee), command.CommitteeId);

        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        if (command.CandidacyIds == null || command.CandidacyIds.Count == 0)
            throw new ValidationException("CandidacyIds", "יש לציין לפחות מועמדות אחת לדיון");

        // Validate all candidacy IDs exist
        foreach (var candidacyId in command.CandidacyIds)
        {
            _ = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken)
                ?? throw new NotFoundException(nameof(Candidacy), candidacyId);
        }

        var entity = new CommitteeMeeting
        {
            CommitteeId = command.CommitteeId,
            OrgUnitId = command.OrgUnitId,
            ScheduledDate = command.ScheduledDate,
            Location = command.Location,
            Status = MeetingStatus.Scheduled,
            CandidacyIdsJson = JsonSerializer.Serialize(command.CandidacyIds)
        };

        await _meetingRepository.AddAsync(entity, cancellationToken);
        return ToMeetingDto(entity);
    }

    public async Task<CommitteeMeetingDto> GetMeetingAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var entity = await _meetingRepository.GetByIdAsync(meetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommitteeMeeting), meetingId);

        return ToMeetingDto(entity);
    }

    public async Task<IEnumerable<CommitteeMeetingDto>> ListMeetingsAsync(int committeeId, CancellationToken cancellationToken = default)
    {
        _ = await _committeeRepository.GetByIdAsync(committeeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Committee), committeeId);

        var results = await _meetingRepository.FindAsync(
            m => m.CommitteeId == committeeId, cancellationToken);

        return results.Select(ToMeetingDto);
    }

    // --- Decisions ---

    public async Task<CommitteeDecisionDto> RecordDecisionAsync(RecordDecisionCommand command, CancellationToken cancellationToken = default)
    {
        var meeting = await _meetingRepository.GetByIdAsync(command.MeetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommitteeMeeting), command.MeetingId);

        var candidacyIds = GetCandidacyIds(meeting);
        if (!candidacyIds.Contains(command.CandidacyId))
            throw new ValidationException("CandidacyId", "המועמדות אינה ברשימת הדיון של ישיבה זו");

        // Check if a decision already exists for this candidacy in this meeting
        var existingDecisions = await _decisionRepository.FindAsync(
            d => d.MeetingId == command.MeetingId && d.CandidacyId == command.CandidacyId,
            cancellationToken);

        if (existingDecisions.Any())
            throw new ValidationException("CandidacyId", "כבר קיימת החלטה למועמדות זו בישיבה זו");

        var decision = new CommitteeDecision
        {
            MeetingId = command.MeetingId,
            CandidacyId = command.CandidacyId,
            Decision = command.Decision,
            Recommendation = command.Recommendation,
            DecidedBy = command.DecidedBy,
            DecidedAt = DateTime.UtcNow
        };

        await _decisionRepository.AddAsync(decision, cancellationToken);

        // Auto-update candidacy status based on decision
        if (command.Decision != CommitteeDecisionType.Deferred)
        {
            await UpdateCandidacyStatusAsync(command.CandidacyId, command.Decision, meeting.OrgUnitId, cancellationToken);
        }

        return ToDecisionDto(decision);
    }

    public async Task<IEnumerable<CommitteeDecisionDto>> GetDecisionsAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        _ = await _meetingRepository.GetByIdAsync(meetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommitteeMeeting), meetingId);

        var decisions = await _decisionRepository.FindAsync(
            d => d.MeetingId == meetingId, cancellationToken);

        return decisions.Select(ToDecisionDto);
    }

    // --- Appeals ---

    public async Task<CommitteeAppealDto> SubmitAppealAsync(SubmitCommitteeAppealCommand command, CancellationToken cancellationToken = default)
    {
        var meeting = await _meetingRepository.GetByIdAsync(command.MeetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommitteeMeeting), command.MeetingId);

        _ = await _candidacyRepository.GetByIdAsync(command.CandidacyId, cancellationToken)
            ?? throw new NotFoundException(nameof(Candidacy), command.CandidacyId);

        if (string.IsNullOrWhiteSpace(command.Reason))
            throw new ValidationException("Reason", "יש לציין סיבת ערעור");

        // Verify candidacy was discussed in this meeting
        var candidacyIds = GetCandidacyIds(meeting);
        if (!candidacyIds.Contains(command.CandidacyId))
            throw new ValidationException("CandidacyId", "המועמדות אינה ברשימת הדיון של ישיבה זו");

        // Check for existing appeal for same candidacy in same meeting
        var existingAppeals = await _appealRepository.FindAsync(
            a => a.MeetingId == command.MeetingId && a.CandidacyId == command.CandidacyId,
            cancellationToken);

        if (existingAppeals.Any())
            throw new ValidationException("CandidacyId", "כבר קיים ערעור למועמדות זו בישיבה זו");

        var appeal = new CommitteeAppeal
        {
            MeetingId = command.MeetingId,
            CandidacyId = command.CandidacyId,
            Reason = command.Reason
        };

        await _appealRepository.AddAsync(appeal, cancellationToken);
        return ToAppealDto(appeal);
    }

    public async Task<CommitteeAppealDto> ResolveAppealAsync(int appealId, string result, CancellationToken cancellationToken = default)
    {
        var appeal = await _appealRepository.GetByIdAsync(appealId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommitteeAppeal), appealId);

        if (appeal.ResolvedAt.HasValue)
            throw new ValidationException("AppealId", "ערעור זה כבר טופל");

        if (string.IsNullOrWhiteSpace(result))
            throw new ValidationException("Result", "יש לציין תוצאת ערעור");

        appeal.Result = result;
        appeal.ResolvedAt = DateTime.UtcNow;

        await _appealRepository.UpdateAsync(appeal, cancellationToken);
        return ToAppealDto(appeal);
    }

    // --- Protocol ---

    public async Task<string> GenerateProtocolAsync(int meetingId, CancellationToken cancellationToken = default)
    {
        var meeting = await _meetingRepository.GetByIdAsync(meetingId, cancellationToken)
            ?? throw new NotFoundException(nameof(CommitteeMeeting), meetingId);

        var committee = await _committeeRepository.GetByIdAsync(meeting.CommitteeId, cancellationToken)
            ?? throw new NotFoundException(nameof(Committee), meeting.CommitteeId);

        var decisions = await _decisionRepository.FindAsync(
            d => d.MeetingId == meetingId, cancellationToken);

        var appeals = await _appealRepository.FindAsync(
            a => a.MeetingId == meetingId, cancellationToken);

        var candidacyIds = GetCandidacyIds(meeting);

        var html = $@"<!DOCTYPE html>
<html dir=""rtl"" lang=""he"">
<head><meta charset=""utf-8""><title>פרוטוקול ישיבת ועדה</title>
<style>
body {{ font-family: Arial, sans-serif; direction: rtl; padding: 20px; }}
h1, h2, h3 {{ color: #333; }}
table {{ border-collapse: collapse; width: 100%; margin: 10px 0; }}
th, td {{ border: 1px solid #ccc; padding: 8px; text-align: right; }}
th {{ background-color: #f0f0f0; }}
.section {{ margin-bottom: 20px; }}
</style></head>
<body>
<h1>פרוטוקול ישיבת ועדה</h1>
<div class=""section"">
<h2>פרטי ישיבה</h2>
<table>
<tr><th>ועדה</th><td>{committee.Name}</td></tr>
<tr><th>תאריך</th><td>{meeting.ScheduledDate:dd/MM/yyyy HH:mm}</td></tr>
<tr><th>מיקום</th><td>{meeting.Location ?? "לא צוין"}</td></tr>
<tr><th>סטטוס</th><td>{meeting.Status}</td></tr>
</table>
</div>
<div class=""section"">
<h2>מועמדויות שנדונו</h2>
<p>סה""כ מועמדויות: {candidacyIds.Count}</p>
<p>מזהי מועמדויות: {string.Join(", ", candidacyIds)}</p>
</div>
<div class=""section"">
<h2>החלטות</h2>";

        var decisionList = decisions.ToList();
        if (decisionList.Count == 0)
        {
            html += "\n<p>לא נרשמו החלטות.</p>";
        }
        else
        {
            html += @"
<table>
<tr><th>מועמדות</th><th>החלטה</th><th>המלצה</th><th>מחליט</th><th>תאריך</th></tr>";
            foreach (var d in decisionList)
            {
                html += $@"
<tr><td>{d.CandidacyId}</td><td>{d.Decision}</td><td>{d.Recommendation ?? "-"}</td><td>{d.DecidedBy}</td><td>{d.DecidedAt:dd/MM/yyyy HH:mm}</td></tr>";
            }
            html += "\n</table>";
        }

        html += "\n</div>\n<div class=\"section\">\n<h2>ערעורים</h2>";

        var appealList = appeals.ToList();
        if (appealList.Count == 0)
        {
            html += "\n<p>לא הוגשו ערעורים.</p>";
        }
        else
        {
            html += @"
<table>
<tr><th>מועמדות</th><th>סיבה</th><th>תוצאה</th><th>תאריך טיפול</th></tr>";
            foreach (var a in appealList)
            {
                html += $@"
<tr><td>{a.CandidacyId}</td><td>{a.Reason}</td><td>{a.Result ?? "טרם טופל"}</td><td>{(a.ResolvedAt.HasValue ? a.ResolvedAt.Value.ToString("dd/MM/yyyy HH:mm") : "-")}</td></tr>";
            }
            html += "\n</table>";
        }

        html += @"
</div>
</body>
</html>";

        return html;
    }

    // --- Private helpers ---

    private async Task UpdateCandidacyStatusAsync(int candidacyId, CommitteeDecisionType decision, int orgUnitId, CancellationToken cancellationToken)
    {
        var candidacy = await _candidacyRepository.GetByIdAsync(candidacyId, cancellationToken);
        if (candidacy == null || !candidacy.CurrentStatusId.HasValue || !candidacy.IsActive)
            return;

        var targetStatusCode = decision == CommitteeDecisionType.Accepted ? "התקבל" : "נדחה";
        var targetStatusCodeEn = decision == CommitteeDecisionType.Accepted ? "accepted" : "rejected";

        var targetStatuses = await _statusRepository.FindAsync(
            s => s.OrgUnitId == orgUnitId
                 && (s.Code == targetStatusCode || s.Code == targetStatusCodeEn),
            cancellationToken);

        var targetStatus = targetStatuses.FirstOrDefault();
        if (targetStatus == null)
            return;

        // Verify the transition is allowed
        var transitions = await _transitionRepository.FindAsync(
            t => t.OrgUnitId == orgUnitId
                 && t.FromStatusId == candidacy.CurrentStatusId
                 && t.ToStatusId == targetStatus.Id,
            cancellationToken);

        if (!transitions.Any())
            return;

        // Record history
        var history = new CandidacyStatusHistory
        {
            CandidacyId = candidacy.Id,
            FromStatusId = candidacy.CurrentStatusId,
            ToStatusId = targetStatus.Id,
            Reason = $"עדכון אוטומטי - החלטת ועדה: {decision}",
            ChangedAt = DateTime.UtcNow
        };

        candidacy.CurrentStatusId = targetStatus.Id;
        if (targetStatus.IsFinal)
            candidacy.IsActive = false;

        await _candidacyRepository.UpdateAsync(candidacy, cancellationToken);
        await _historyRepository.AddAsync(history, cancellationToken);
    }

    private static List<int> GetCandidacyIds(CommitteeMeeting meeting)
    {
        try
        {
            return JsonSerializer.Deserialize<List<int>>(meeting.CandidacyIdsJson) ?? new List<int>();
        }
        catch
        {
            return new List<int>();
        }
    }

    private static List<CommitteeMemberInfo> GetMembers(Committee entity)
    {
        try
        {
            return JsonSerializer.Deserialize<List<CommitteeMemberInfo>>(entity.MembersJson) ?? new List<CommitteeMemberInfo>();
        }
        catch
        {
            return new List<CommitteeMemberInfo>();
        }
    }

    private static CommitteeDto ToDto(Committee entity) =>
        new(entity.Id, entity.OrgUnitId, entity.Name, entity.Description,
            GetMembers(entity), entity.CreatedAt);

    private static CommitteeMeetingDto ToMeetingDto(CommitteeMeeting entity) =>
        new(entity.Id, entity.CommitteeId, entity.OrgUnitId, entity.ScheduledDate,
            entity.Location, entity.Status, GetCandidacyIds(entity), entity.CreatedAt);

    private static CommitteeDecisionDto ToDecisionDto(CommitteeDecision entity) =>
        new(entity.Id, entity.MeetingId, entity.CandidacyId, entity.Decision,
            entity.Recommendation, entity.DecidedBy, entity.DecidedAt);

    private static CommitteeAppealDto ToAppealDto(CommitteeAppeal entity) =>
        new(entity.Id, entity.MeetingId, entity.CandidacyId, entity.Reason,
            entity.Result, entity.ResolvedAt, entity.CreatedAt);
}
