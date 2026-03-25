using System.Linq.Expressions;
using CandidacyManagement.Application.Calendar;
using CandidacyManagement.Application.Interviews;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Interviews;

public class InterviewServiceTests
{
    private readonly Mock<IRepository<Interview>> _interviewRepoMock;
    private readonly Mock<IRepository<InterviewFeedback>> _feedbackRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<CallForCandidates>> _callRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<StatusTransition>> _transitionRepoMock;
    private readonly Mock<IRepository<CandidacyStatusHistory>> _historyRepoMock;
    private readonly Mock<ICalendarService> _calendarServiceMock;
    private readonly InterviewService _sut;

    public InterviewServiceTests()
    {
        _interviewRepoMock = new Mock<IRepository<Interview>>();
        _feedbackRepoMock = new Mock<IRepository<InterviewFeedback>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _callRepoMock = new Mock<IRepository<CallForCandidates>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        _historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();
        _calendarServiceMock = new Mock<ICalendarService>();

        _calendarServiceMock
            .Setup(c => c.SendInterviewInviteAsync(It.IsAny<Interview>(), It.IsAny<List<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(string.Empty);

        _sut = new InterviewService(
            _interviewRepoMock.Object,
            _feedbackRepoMock.Object,
            _candidacyRepoMock.Object,
            _callRepoMock.Object,
            _orgUnitRepoMock.Object,
            _statusRepoMock.Object,
            _transitionRepoMock.Object,
            _historyRepoMock.Object,
            _calendarServiceMock.Object);
    }

    #region CreateAsync

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsInterviewDto()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 5 });
        _interviewRepoMock.Setup(r => r.AddAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Interview e, CancellationToken _) => { e.Id = 100; return e; });

        var command = new CreateInterviewCommand(
            OrgUnitId: 1,
            CallForCandidatesId: 10,
            CandidacyId: 5,
            ScheduledDate: DateTime.UtcNow.AddDays(7),
            StartTime: new TimeSpan(9, 0, 0),
            EndTime: new TimeSpan(10, 0, 0),
            Location: "ירושלים",
            InterviewerIds: new List<int> { 101, 102 },
            InterviewType: InterviewType.First);

        var result = await _sut.CreateAsync(command);

        result.Should().NotBeNull();
        result.OrgUnitId.Should().Be(1);
        result.CandidacyId.Should().Be(5);
        result.InterviewerIds.Should().BeEquivalentTo(new List<int> { 101, 102 });
        result.Status.Should().Be(InterviewStatus.Scheduled);
        result.InterviewType.Should().Be(InterviewType.First);
    }

    [Fact]
    public async Task CreateAsync_OrgUnitNotFound_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var command = new CreateInterviewCommand(
            999, 10, 5, DateTime.UtcNow, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0),
            null, new List<int> { 1 }, InterviewType.First);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateAsync_EmptyInterviewerList_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 5 });

        var command = new CreateInterviewCommand(
            1, 10, 5, DateTime.UtcNow, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0),
            null, new List<int>(), InterviewType.First);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_EndTimeBeforeStartTime_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 5 });

        var command = new CreateInterviewCommand(
            1, 10, 5, DateTime.UtcNow,
            StartTime: new TimeSpan(10, 0, 0),
            EndTime: new TimeSpan(9, 0, 0),
            null, new List<int> { 1 }, InterviewType.First);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateAsync

    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsUpdatedDto()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1, CandidacyId = 5,
            Status = InterviewStatus.Scheduled,
            InterviewerIdsJson = "[101]",
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(10, 0, 0)
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        var command = new UpdateInterviewCommand(
            Id: 1,
            ScheduledDate: DateTime.UtcNow.AddDays(14),
            StartTime: new TimeSpan(11, 0, 0),
            EndTime: new TimeSpan(12, 0, 0),
            Location: "תל אביב",
            InterviewerIds: new List<int> { 101, 103 });

        var result = await _sut.UpdateAsync(command);

        result.Location.Should().Be("תל אביב");
        result.InterviewerIds.Should().BeEquivalentTo(new List<int> { 101, 103 });
        result.StartTime.Should().Be(new TimeSpan(11, 0, 0));
    }

    [Fact]
    public async Task UpdateAsync_CompletedInterview_ThrowsValidationException()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1,
            Status = InterviewStatus.Completed,
            InterviewerIdsJson = "[101]"
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        var command = new UpdateInterviewCommand(
            1, DateTime.UtcNow, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0),
            null, new List<int> { 101 });

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UpdateAsync_InterviewNotFound_ThrowsNotFoundException()
    {
        _interviewRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Interview?)null);

        var command = new UpdateInterviewCommand(
            999, DateTime.UtcNow, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0),
            null, new List<int> { 1 });

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region SubmitFeedbackAsync

    [Fact]
    public async Task SubmitFeedbackAsync_ValidFeedback_ReturnsFeedbackDto()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1, CandidacyId = 5,
            Status = InterviewStatus.Scheduled,
            InterviewerIdsJson = "[101,102]"
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);
        _feedbackRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InterviewFeedback, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<InterviewFeedback>());
        _feedbackRepoMock.Setup(r => r.AddAsync(It.IsAny<InterviewFeedback>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InterviewFeedback f, CancellationToken _) => { f.Id = 50; return f; });

        var command = new SubmitFeedbackCommand(
            InterviewId: 1, InterviewerId: 101, Rating: 7.5m, Comments: "מועמד מצוין");

        var result = await _sut.SubmitFeedbackAsync(command);

        result.Should().NotBeNull();
        result.InterviewerId.Should().Be(101);
        result.Rating.Should().Be(7.5m);
        result.Comments.Should().Be("מועמד מצוין");
    }

    [Fact]
    public async Task SubmitFeedbackAsync_InterviewerNotAssigned_ThrowsValidationException()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1,
            InterviewerIdsJson = "[101,102]"
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        var command = new SubmitFeedbackCommand(1, InterviewerId: 999, Rating: 5m, Comments: null);

        var act = () => _sut.SubmitFeedbackAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitFeedbackAsync_DuplicateFeedback_ThrowsValidationException()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1,
            InterviewerIdsJson = "[101,102]"
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        // First call checks interviewer assignment (returns empty - no existing feedback for this interviewer)
        // But we need to simulate that the duplicate check finds existing feedback
        _feedbackRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InterviewFeedback, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<InterviewFeedback>
            {
                new() { Id = 50, InterviewId = 1, InterviewerId = 101, Rating = 8m }
            });

        var command = new SubmitFeedbackCommand(1, InterviewerId: 101, Rating: 9m, Comments: null);

        var act = () => _sut.SubmitFeedbackAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitFeedbackAsync_RatingAbove10_ThrowsValidationException()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1,
            InterviewerIdsJson = "[101]"
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        var command = new SubmitFeedbackCommand(1, InterviewerId: 101, Rating: 11m, Comments: null);

        var act = () => _sut.SubmitFeedbackAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitFeedbackAsync_NegativeRating_ThrowsValidationException()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1,
            InterviewerIdsJson = "[101]"
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);

        var command = new SubmitFeedbackCommand(1, InterviewerId: 101, Rating: -1m, Comments: null);

        var act = () => _sut.SubmitFeedbackAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Auto Status Update

    [Fact]
    public async Task SubmitFeedbackAsync_AllInterviewersSubmitted_InterviewStatusChangesToCompleted()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1, CandidacyId = 5,
            Status = InterviewStatus.Scheduled,
            InterviewerIdsJson = "[101,102]"
        };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };

        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        // First FindAsync call: check for duplicate feedback from interviewer 102 (none)
        // Second FindAsync call: get all feedbacks for the interview (both 101 and 102)
        var callCount = 0;
        _feedbackRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InterviewFeedback, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<InterviewFeedback, bool>> pred, CancellationToken _) =>
            {
                callCount++;
                if (callCount == 1)
                    return Enumerable.Empty<InterviewFeedback>(); // No duplicate
                // All feedbacks (interviewer 101 already submitted, 102 is submitting now)
                return new List<InterviewFeedback>
                {
                    new() { Id = 50, InterviewId = 1, InterviewerId = 101, Rating = 7m },
                    new() { Id = 51, InterviewId = 1, InterviewerId = 102, Rating = 8m }
                };
            });
        _feedbackRepoMock.Setup(r => r.AddAsync(It.IsAny<InterviewFeedback>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InterviewFeedback f, CancellationToken _) => { f.Id = 51; return f; });

        var passedStatus = new StatusDefinition { Id = 20, OrgUnitId = 1, Code = "passed_interview", IsFinal = false };
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition> { passedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusTransition> { new() { Id = 1, FromStatusId = 10, ToStatusId = 20, OrgUnitId = 1 } });

        var command = new SubmitFeedbackCommand(1, InterviewerId: 102, Rating: 8m, Comments: null);

        await _sut.SubmitFeedbackAsync(command);

        interview.Status.Should().Be(InterviewStatus.Completed);
        _interviewRepoMock.Verify(r => r.UpdateAsync(interview, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_AverageRatingAboveThreshold_CandidacyPassesInterview()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1, CandidacyId = 5,
            Status = InterviewStatus.Scheduled,
            InterviewerIdsJson = "[101,102]"
        };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };

        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        var callCount = 0;
        _feedbackRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InterviewFeedback, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<InterviewFeedback, bool>> pred, CancellationToken _) =>
            {
                callCount++;
                if (callCount == 1)
                    return Enumerable.Empty<InterviewFeedback>();
                // Average = (7 + 8) / 2 = 7.5 >= 5.0 → passed_interview
                return new List<InterviewFeedback>
                {
                    new() { Id = 50, InterviewId = 1, InterviewerId = 101, Rating = 7m },
                    new() { Id = 51, InterviewId = 1, InterviewerId = 102, Rating = 8m }
                };
            });
        _feedbackRepoMock.Setup(r => r.AddAsync(It.IsAny<InterviewFeedback>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InterviewFeedback f, CancellationToken _) => { f.Id = 51; return f; });

        var passedStatus = new StatusDefinition { Id = 20, OrgUnitId = 1, Code = "passed_interview", IsFinal = false };
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition> { passedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusTransition> { new() { Id = 1, FromStatusId = 10, ToStatusId = 20, OrgUnitId = 1 } });

        var command = new SubmitFeedbackCommand(1, InterviewerId: 102, Rating: 8m, Comments: null);

        await _sut.SubmitFeedbackAsync(command);

        candidacy.CurrentStatusId.Should().Be(20);
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitFeedbackAsync_AverageRatingBelowThreshold_CandidacyFailsInterview()
    {
        var interview = new Interview
        {
            Id = 1, OrgUnitId = 1, CandidacyId = 5,
            Status = InterviewStatus.Scheduled,
            InterviewerIdsJson = "[101,102]"
        };
        var candidacy = new Candidacy { Id = 5, OrgUnitId = 1, CurrentStatusId = 10, IsActive = true };

        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(interview);
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        var callCount = 0;
        _feedbackRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<InterviewFeedback, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Expression<Func<InterviewFeedback, bool>> pred, CancellationToken _) =>
            {
                callCount++;
                if (callCount == 1)
                    return Enumerable.Empty<InterviewFeedback>();
                // Average = (2 + 3) / 2 = 2.5 < 5.0 → failed_interview
                return new List<InterviewFeedback>
                {
                    new() { Id = 50, InterviewId = 1, InterviewerId = 101, Rating = 2m },
                    new() { Id = 51, InterviewId = 1, InterviewerId = 102, Rating = 3m }
                };
            });
        _feedbackRepoMock.Setup(r => r.AddAsync(It.IsAny<InterviewFeedback>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((InterviewFeedback f, CancellationToken _) => { f.Id = 51; return f; });

        var failedStatus = new StatusDefinition { Id = 30, OrgUnitId = 1, Code = "failed_interview", IsFinal = true };
        _statusRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition> { failedStatus });
        _transitionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusTransition> { new() { Id = 1, FromStatusId = 10, ToStatusId = 30, OrgUnitId = 1 } });

        var command = new SubmitFeedbackCommand(1, InterviewerId: 102, Rating: 3m, Comments: null);

        await _sut.SubmitFeedbackAsync(command);

        candidacy.CurrentStatusId.Should().Be(30);
        candidacy.IsActive.Should().BeFalse();
        _historyRepoMock.Verify(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region ScheduleSecondInterviewAsync

    [Fact]
    public async Task ScheduleSecondInterviewAsync_FromFirstInterview_CreatesSecondInterview()
    {
        var firstInterview = new Interview
        {
            Id = 1, OrgUnitId = 1, CallForCandidatesId = 10, CandidacyId = 5,
            InterviewType = InterviewType.First,
            Status = InterviewStatus.Completed,
            InterviewerIdsJson = "[101]"
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(firstInterview);
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _callRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 10 });
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 5 });
        _interviewRepoMock.Setup(r => r.AddAsync(It.IsAny<Interview>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Interview e, CancellationToken _) => { e.Id = 200; return e; });

        var command = new CreateInterviewCommand(
            OrgUnitId: 1, CallForCandidatesId: 10, CandidacyId: 5,
            ScheduledDate: DateTime.UtcNow.AddDays(14),
            StartTime: new TimeSpan(14, 0, 0),
            EndTime: new TimeSpan(15, 0, 0),
            Location: "חיפה",
            InterviewerIds: new List<int> { 201, 202 },
            InterviewType: InterviewType.First); // Will be overridden to Second

        var result = await _sut.ScheduleSecondInterviewAsync(1, command);

        result.Should().NotBeNull();
        result.InterviewType.Should().Be(InterviewType.Second);
        result.CandidacyId.Should().Be(5);
    }

    [Fact]
    public async Task ScheduleSecondInterviewAsync_FromNonFirstInterview_ThrowsValidationException()
    {
        var secondInterview = new Interview
        {
            Id = 2, OrgUnitId = 1,
            InterviewType = InterviewType.Second,
            InterviewerIdsJson = "[101]"
        };
        _interviewRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(secondInterview);

        var command = new CreateInterviewCommand(
            1, 10, 5, DateTime.UtcNow, new TimeSpan(9, 0, 0), new TimeSpan(10, 0, 0),
            null, new List<int> { 1 }, InterviewType.First);

        var act = () => _sut.ScheduleSecondInterviewAsync(2, command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion
}
