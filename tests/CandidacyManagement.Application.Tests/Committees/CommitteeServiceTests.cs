using System.Linq.Expressions;
using CandidacyManagement.Application.Committees;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Committees;

public class CommitteeServiceTests
{
    private readonly Mock<IRepository<Committee>> _committeeRepoMock;
    private readonly Mock<IRepository<CommitteeMeeting>> _meetingRepoMock;
    private readonly Mock<IRepository<CommitteeDecision>> _decisionRepoMock;
    private readonly Mock<IRepository<CommitteeAppeal>> _appealRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<StatusTransition>> _transitionRepoMock;
    private readonly Mock<IRepository<CandidacyStatusHistory>> _historyRepoMock;
    private readonly CommitteeService _sut;

    public CommitteeServiceTests()
    {
        _committeeRepoMock = new Mock<IRepository<Committee>>();
        _meetingRepoMock = new Mock<IRepository<CommitteeMeeting>>();
        _decisionRepoMock = new Mock<IRepository<CommitteeDecision>>();
        _appealRepoMock = new Mock<IRepository<CommitteeAppeal>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        _historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();

        _sut = new CommitteeService(
            _committeeRepoMock.Object,
            _meetingRepoMock.Object,
            _decisionRepoMock.Object,
            _appealRepoMock.Object,
            _candidacyRepoMock.Object,
            _orgUnitRepoMock.Object,
            _statusRepoMock.Object,
            _transitionRepoMock.Object,
            _historyRepoMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsCommitteeDto()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _committeeRepoMock.Setup(r => r.AddAsync(It.IsAny<Committee>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Committee e, CancellationToken _) => { e.Id = 10; return e; });

        var command = new CreateCommitteeCommand(
            OrgUnitId: 1,
            Name: "ועדת מיון",
            Description: "ועדה לבחירת מועמדים",
            Members: new List<CommitteeMemberInfo>
            {
                new(MemberId: 101, Role: "יו\"ר"),
                new(MemberId: 102, Role: "חבר")
            });

        var result = await _sut.CreateAsync(command);

        result.Should().NotBeNull();
        result.OrgUnitId.Should().Be(1);
        result.Name.Should().Be("ועדת מיון");
        result.Members.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAsync_EmptyName_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateCommitteeCommand(
            OrgUnitId: 1,
            Name: "",
            Description: null,
            Members: new List<CommitteeMemberInfo> { new(1, "חבר") });

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_EmptyMembers_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateCommitteeCommand(
            OrgUnitId: 1,
            Name: "ועדה",
            Description: null,
            Members: new List<CommitteeMemberInfo>());

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region CreateMeetingAsync

    [Fact]
    public async Task CreateMeetingAsync_WithValidCommand_ReturnsMeetingDto()
    {
        _committeeRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Committee { Id = 10, OrgUnitId = 1 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new Candidacy { Id = id });
        _meetingRepoMock.Setup(r => r.AddAsync(It.IsAny<CommitteeMeeting>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommitteeMeeting e, CancellationToken _) => { e.Id = 50; return e; });

        var command = new CreateMeetingCommand(
            CommitteeId: 10,
            OrgUnitId: 1,
            ScheduledDate: DateTime.UtcNow.AddDays(7),
            Location: "ירושלים",
            CandidacyIds: new List<int> { 1, 2, 3 });

        var result = await _sut.CreateMeetingAsync(command);

        result.Should().NotBeNull();
        result.CommitteeId.Should().Be(10);
        result.CandidacyIds.Should().BeEquivalentTo(new List<int> { 1, 2, 3 });
        result.Status.Should().Be(MeetingStatus.Scheduled);
    }

    [Fact]
    public async Task CreateMeetingAsync_EmptyCandidacyList_ThrowsValidationException()
    {
        _committeeRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Committee { Id = 10 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });

        var command = new CreateMeetingCommand(
            CommitteeId: 10, OrgUnitId: 1,
            ScheduledDate: DateTime.UtcNow, Location: null,
            CandidacyIds: new List<int>());

        var act = () => _sut.CreateMeetingAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateMeetingAsync_CommitteeNotFound_ThrowsNotFoundException()
    {
        _committeeRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Committee?)null);

        var command = new CreateMeetingCommand(
            CommitteeId: 999, OrgUnitId: 1,
            ScheduledDate: DateTime.UtcNow, Location: null,
            CandidacyIds: new List<int> { 1 });

        var act = () => _sut.CreateMeetingAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region RecordDecisionAsync

    [Fact]
    public async Task RecordDecisionAsync_WithValidCommand_ReturnsDecisionDto()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CommitteeId = 10, OrgUnitId = 1,
            CandidacyIdsJson = "[1,2,3]"
        };
        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _decisionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeDecision, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CommitteeDecision>());
        _decisionRepoMock.Setup(r => r.AddAsync(It.IsAny<CommitteeDecision>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommitteeDecision d, CancellationToken _) => { d.Id = 200; return d; });

        // Setup for auto status update (Deferred won't trigger it, but we use Accepted here)
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        var acceptedStatus = new StatusDefinition { Id = 20, OrgUnitId = 1, Code = "accepted", IsFinal = false };
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition> { acceptedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusTransition> { new() { Id = 1, FromStatusId = 10, ToStatusId = 20, OrgUnitId = 1 } });

        var command = new RecordDecisionCommand(
            MeetingId: 50, CandidacyId: 1,
            Decision: CommitteeDecisionType.Accepted,
            Recommendation: "מועמד מצוין", DecidedBy: 101);

        var result = await _sut.RecordDecisionAsync(command);

        result.Should().NotBeNull();
        result.MeetingId.Should().Be(50);
        result.CandidacyId.Should().Be(1);
        result.Decision.Should().Be(CommitteeDecisionType.Accepted);
        result.Recommendation.Should().Be("מועמד מצוין");
    }

    [Fact]
    public async Task RecordDecisionAsync_CandidacyNotInMeeting_ThrowsValidationException()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CommitteeId = 10, OrgUnitId = 1,
            CandidacyIdsJson = "[1,2,3]"
        };
        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);

        var command = new RecordDecisionCommand(
            MeetingId: 50, CandidacyId: 999,
            Decision: CommitteeDecisionType.Accepted,
            Recommendation: null, DecidedBy: 101);

        var act = () => _sut.RecordDecisionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task RecordDecisionAsync_DuplicateDecision_ThrowsValidationException()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CommitteeId = 10, OrgUnitId = 1,
            CandidacyIdsJson = "[1,2,3]"
        };
        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _decisionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeDecision, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommitteeDecision>
            {
                new() { Id = 200, MeetingId = 50, CandidacyId = 1, Decision = CommitteeDecisionType.Accepted }
            });

        var command = new RecordDecisionCommand(
            MeetingId: 50, CandidacyId: 1,
            Decision: CommitteeDecisionType.Rejected,
            Recommendation: null, DecidedBy: 101);

        var act = () => _sut.RecordDecisionAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Auto Status Update

    [Fact]
    public async Task RecordDecisionAsync_AcceptedDecision_UpdatesCandidacyStatus()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CommitteeId = 10, OrgUnitId = 1,
            CandidacyIdsJson = "[1]"
        };
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };

        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _decisionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeDecision, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CommitteeDecision>());
        _decisionRepoMock.Setup(r => r.AddAsync(It.IsAny<CommitteeDecision>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommitteeDecision d, CancellationToken _) => { d.Id = 200; return d; });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        var acceptedStatus = new StatusDefinition { Id = 20, OrgUnitId = 1, Code = "accepted", IsFinal = false };
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition> { acceptedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusTransition> { new() { Id = 1, FromStatusId = 10, ToStatusId = 20, OrgUnitId = 1 } });

        var command = new RecordDecisionCommand(
            MeetingId: 50, CandidacyId: 1,
            Decision: CommitteeDecisionType.Accepted,
            Recommendation: null, DecidedBy: 101);

        await _sut.RecordDecisionAsync(command);

        candidacy.CurrentStatusId.Should().Be(20);
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordDecisionAsync_RejectedDecision_UpdatesCandidacyStatusAndDeactivates()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CommitteeId = 10, OrgUnitId = 1,
            CandidacyIdsJson = "[1]"
        };
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };

        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _decisionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeDecision, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CommitteeDecision>());
        _decisionRepoMock.Setup(r => r.AddAsync(It.IsAny<CommitteeDecision>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommitteeDecision d, CancellationToken _) => { d.Id = 200; return d; });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        var rejectedStatus = new StatusDefinition { Id = 30, OrgUnitId = 1, Code = "rejected", IsFinal = true };
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition> { rejectedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusTransition> { new() { Id = 1, FromStatusId = 10, ToStatusId = 30, OrgUnitId = 1 } });

        var command = new RecordDecisionCommand(
            MeetingId: 50, CandidacyId: 1,
            Decision: CommitteeDecisionType.Rejected,
            Recommendation: null, DecidedBy: 101);

        await _sut.RecordDecisionAsync(command);

        candidacy.CurrentStatusId.Should().Be(30);
        candidacy.IsActive.Should().BeFalse();
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordDecisionAsync_DeferredDecision_DoesNotUpdateCandidacyStatus()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CommitteeId = 10, OrgUnitId = 1,
            CandidacyIdsJson = "[1]"
        };
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };

        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _decisionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeDecision, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CommitteeDecision>());
        _decisionRepoMock.Setup(r => r.AddAsync(It.IsAny<CommitteeDecision>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommitteeDecision d, CancellationToken _) => { d.Id = 200; return d; });

        var command = new RecordDecisionCommand(
            MeetingId: 50, CandidacyId: 1,
            Decision: CommitteeDecisionType.Deferred,
            Recommendation: "דיון נוסף נדרש", DecidedBy: 101);

        await _sut.RecordDecisionAsync(command);

        candidacy.CurrentStatusId.Should().Be(10); // unchanged
        candidacy.IsActive.Should().BeTrue();
        _candidacyRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Candidacy>(), It.IsAny<CancellationToken>()), Times.Never);
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    #endregion

    #region SubmitAppealAsync

    [Fact]
    public async Task SubmitAppealAsync_WithValidCommand_ReturnsAppealDto()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CommitteeId = 10, OrgUnitId = 1,
            CandidacyIdsJson = "[1,2]"
        };
        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _appealRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeAppeal, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<CommitteeAppeal>());
        _appealRepoMock.Setup(r => r.AddAsync(It.IsAny<CommitteeAppeal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CommitteeAppeal a, CancellationToken _) => { a.Id = 300; return a; });

        var command = new SubmitCommitteeAppealCommand(
            MeetingId: 50, CandidacyId: 1, Reason: "החלטה לא מוצדקת");

        var result = await _sut.SubmitAppealAsync(command);

        result.Should().NotBeNull();
        result.MeetingId.Should().Be(50);
        result.CandidacyId.Should().Be(1);
        result.Reason.Should().Be("החלטה לא מוצדקת");
        result.ResolvedAt.Should().BeNull();
    }

    [Fact]
    public async Task SubmitAppealAsync_EmptyReason_ThrowsValidationException()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CandidacyIdsJson = "[1]"
        };
        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });

        var command = new SubmitCommitteeAppealCommand(
            MeetingId: 50, CandidacyId: 1, Reason: "");

        var act = () => _sut.SubmitAppealAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAppealAsync_DuplicateAppeal_ThrowsValidationException()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CandidacyIdsJson = "[1]"
        };
        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _appealRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeAppeal, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommitteeAppeal>
            {
                new() { Id = 300, MeetingId = 50, CandidacyId = 1, Reason = "ערעור קודם" }
            });

        var command = new SubmitCommitteeAppealCommand(
            MeetingId: 50, CandidacyId: 1, Reason: "ערעור נוסף");

        var act = () => _sut.SubmitAppealAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAppealAsync_CandidacyNotInMeeting_ThrowsValidationException()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CandidacyIdsJson = "[1,2]"
        };
        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 999 });

        var command = new SubmitCommitteeAppealCommand(
            MeetingId: 50, CandidacyId: 999, Reason: "סיבה כלשהי");

        var act = () => _sut.SubmitAppealAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region ResolveAppealAsync

    [Fact]
    public async Task ResolveAppealAsync_WithValidInput_ReturnsResolvedAppealDto()
    {
        var appeal = new CommitteeAppeal
        {
            Id = 300, MeetingId = 50, CandidacyId = 1,
            Reason = "החלטה לא מוצדקת", ResolvedAt = null
        };
        _appealRepoMock.Setup(r => r.GetByIdAsync(300, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appeal);

        var result = await _sut.ResolveAppealAsync(300, "הערעור התקבל");

        result.Should().NotBeNull();
        result.Result.Should().Be("הערעור התקבל");
        result.ResolvedAt.Should().NotBeNull();
        _appealRepoMock.Verify(r => r.UpdateAsync(appeal, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ResolveAppealAsync_AlreadyResolved_ThrowsValidationException()
    {
        var appeal = new CommitteeAppeal
        {
            Id = 300, MeetingId = 50, CandidacyId = 1,
            Reason = "סיבה", Result = "נדחה", ResolvedAt = DateTime.UtcNow.AddDays(-1)
        };
        _appealRepoMock.Setup(r => r.GetByIdAsync(300, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appeal);

        var act = () => _sut.ResolveAppealAsync(300, "תוצאה חדשה");

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region GenerateProtocolAsync

    [Fact]
    public async Task GenerateProtocolAsync_ReturnsHtmlContainingMeetingDetails()
    {
        var meeting = new CommitteeMeeting
        {
            Id = 50, CommitteeId = 10, OrgUnitId = 1,
            ScheduledDate = new DateTime(2024, 6, 15, 10, 0, 0),
            Location = "ירושלים",
            Status = MeetingStatus.Completed,
            CandidacyIdsJson = "[1,2]"
        };
        var committee = new Committee
        {
            Id = 10, OrgUnitId = 1, Name = "ועדת מיון ראשית"
        };

        _meetingRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(meeting);
        _committeeRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(committee);
        _decisionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeDecision, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommitteeDecision>
            {
                new() { Id = 200, MeetingId = 50, CandidacyId = 1, Decision = CommitteeDecisionType.Accepted, Recommendation = "מומלץ", DecidedBy = 101, DecidedAt = new DateTime(2024, 6, 15, 11, 0, 0) },
                new() { Id = 201, MeetingId = 50, CandidacyId = 2, Decision = CommitteeDecisionType.Rejected, Recommendation = null, DecidedBy = 101, DecidedAt = new DateTime(2024, 6, 15, 11, 30, 0) }
            });
        _appealRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<CommitteeAppeal, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CommitteeAppeal>
            {
                new() { Id = 300, MeetingId = 50, CandidacyId = 2, Reason = "החלטה שגויה", Result = "נדחה", ResolvedAt = new DateTime(2024, 6, 20), CreatedAt = new DateTime(2024, 6, 16) }
            });

        var html = await _sut.GenerateProtocolAsync(50);

        html.Should().Contain("ועדת מיון ראשית");
        html.Should().Contain("ירושלים");
        html.Should().Contain("Accepted");
        html.Should().Contain("Rejected");
        html.Should().Contain("מומלץ");
        html.Should().Contain("החלטה שגויה");
        html.Should().Contain("פרוטוקול");
    }

    #endregion
}
