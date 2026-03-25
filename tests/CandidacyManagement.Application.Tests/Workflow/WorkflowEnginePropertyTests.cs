using System.Linq.Expressions;
using CandidacyManagement.Application.Workflow;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using FsCheck;
using FsCheck.Xunit;
using MediatR;
using Moq;

namespace CandidacyManagement.Application.Tests.Workflow;

/// <summary>
/// Feature: unified-candidacy-management, Property 1: State Machine Transition Enforcement
/// 
/// **Validates: Requirements 1.5, 3.3**
/// 
/// For any candidacy and any target status code, if ExecuteTransitionAsync returns Success,
/// then the transition (fromStatus → toStatus) must exist in the StatusTransition table for that org unit.
/// Additionally, if the current status is final, no transition should succeed.
/// </summary>
public class WorkflowEnginePropertyTests
{
    /// <summary>
    /// Data container for a generated workflow scenario.
    /// </summary>
    public record WorkflowScenario(
        List<StatusDefinition> Statuses,
        List<StatusTransition> Transitions,
        Candidacy Candidacy,
        string TargetStatusCode);

    /// <summary>
    /// Custom Arbitrary that generates valid workflow scenarios:
    /// - A set of StatusDefinitions (at least one initial, at least one final)
    /// - A set of StatusTransitions between those statuses
    /// - A Candidacy with a random current status from the set
    /// - A random target status code (may or may not be a valid transition)
    /// </summary>
    private static Arbitrary<WorkflowScenario> WorkflowScenarioArb()
    {
        return Arb.From(
            from orgUnitId in Gen.Choose(1, 100)
            from statusCount in Gen.Choose(3, 8)
            let statusIds = Enumerable.Range(1, statusCount).ToList()
            from initialIdx in Gen.Choose(0, 0) // first status is always initial
            from finalIdx in Gen.Choose(statusCount - 1, statusCount - 1) // last status is always final
            let statuses = statusIds.Select((id, idx) => new StatusDefinition
            {
                Id = id,
                OrgUnitId = orgUnitId,
                Code = $"status_{id}",
                DisplayName = $"Status {id}",
                Category = CandidacyStatusCategory.InReview,
                IsFinal = idx == finalIdx,
                IsInitial = idx == initialIdx,
                SortOrder = idx
            }).ToList()
            // Generate transitions: each non-final status can transition to 1-3 other statuses
            from transitionSets in Gen.Sequence(
                statuses.Where(s => !s.IsFinal).Select(fromStatus =>
                    from transCount in Gen.Choose(1, Math.Min(3, statuses.Count - 1))
                    from toIndices in Gen.ArrayOf(transCount, Gen.Choose(0, statuses.Count - 1))
                    let distinctToIds = toIndices.Distinct()
                        .Where(i => statuses[i].Id != fromStatus.Id)
                        .Take(transCount)
                    select distinctToIds.Select((toIdx, tIdx) => new StatusTransition
                    {
                        Id = fromStatus.Id * 100 + tIdx + 1,
                        OrgUnitId = orgUnitId,
                        FromStatusId = fromStatus.Id,
                        ToStatusId = statuses[toIdx].Id,
                        RequiresReason = false
                    }).ToList()
                ))
            let transitions = transitionSets.SelectMany(t => t).ToList()
            // Pick a random current status for the candidacy
            from currentStatusIdx in Gen.Choose(0, statuses.Count - 1)
            let currentStatus = statuses[currentStatusIdx]
            // Pick a random target status code (could be valid or invalid)
            from targetStatusIdx in Gen.Choose(0, statuses.Count - 1)
            let targetStatus = statuses[targetStatusIdx]
            let candidacy = new Candidacy
            {
                Id = 1,
                OrgUnitId = orgUnitId,
                ContactId = 1,
                CallForCandidatesId = 1,
                CurrentStatusId = currentStatus.Id,
                IsActive = !currentStatus.IsFinal
            }
            select new WorkflowScenario(statuses, transitions, candidacy, targetStatus.Code));
    }

    private static (WorkflowEngine engine, Mock<IRepository<Candidacy>> candidacyRepo) SetupEngine(WorkflowScenario scenario)
    {
        var candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        var statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        var transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        var historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();
        var workflowRepoMock = new Mock<IRepository<WorkflowDefinition>>();
        var mediatorMock = new Mock<IMediator>();

        // Setup candidacy repo
        candidacyRepoMock
            .Setup(r => r.GetByIdAsync(scenario.Candidacy.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(scenario.Candidacy);

        // Setup status repo - GetByIdAsync
        foreach (var status in scenario.Statuses)
        {
            statusRepoMock
                .Setup(r => r.GetByIdAsync(status.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(status);
        }

        // Setup status repo - FindAsync (used by GetStatusByCodeAsync)
        statusRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<StatusDefinition, bool>> predicate, CancellationToken _) =>
            {
                var compiled = predicate.Compile();
                var result = scenario.Statuses.Where(compiled).ToList();
                return Task.FromResult<IEnumerable<StatusDefinition>>(result);
            });

        // Setup transition repo - FindAsync
        transitionRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<StatusTransition, bool>> predicate, CancellationToken _) =>
            {
                var compiled = predicate.Compile();
                var result = scenario.Transitions.Where(compiled).ToList();
                return Task.FromResult<IEnumerable<StatusTransition>>(result);
            });

        // Setup history repo
        historyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CandidacyStatusHistory h, CancellationToken _) => h);

        var engine = new WorkflowEngine(
            candidacyRepoMock.Object,
            statusRepoMock.Object,
            transitionRepoMock.Object,
            historyRepoMock.Object,
            workflowRepoMock.Object,
            mediatorMock.Object);

        return (engine, candidacyRepoMock);
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 1: State Machine Transition Enforcement
    /// **Validates: Requirements 1.5, 3.3**
    /// 
    /// If ExecuteTransitionAsync returns Success, then the transition (fromStatus → toStatus)
    /// must exist in the allowed transitions list for that org unit.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(WorkflowEnginePropertyTests) })]
    public async Task<bool> SuccessfulTransitionMustExistInAllowedTransitions(WorkflowScenario scenario)
    {
        var (engine, _) = SetupEngine(scenario);

        var result = await engine.ExecuteTransitionAsync(
            scenario.Candidacy.Id,
            scenario.TargetStatusCode,
            "property test reason",
            userId: 1);

        if (!result.IsSuccess)
            return true; // vacuously true: we only care about successful transitions

        // Find the target status
        var targetStatus = scenario.Statuses.First(s => s.Code == scenario.TargetStatusCode);
        var fromStatusId = scenario.Candidacy.CurrentStatusId!.Value;

        // Verify the transition exists in the allowed transitions
        var transitionExists = scenario.Transitions.Any(t =>
            t.OrgUnitId == scenario.Candidacy.OrgUnitId &&
            t.FromStatusId == fromStatusId &&
            t.ToStatusId == targetStatus.Id);

        return transitionExists;
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 1: State Machine Transition Enforcement (Final Status)
    /// **Validates: Requirements 1.5, 3.3**
    /// 
    /// If the current status is final, no transition should succeed
    /// (candidacy becomes inactive).
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(WorkflowEnginePropertyTests) })]
    public async Task<bool> FinalStatusBlocksAllTransitions(WorkflowScenario scenario)
    {
        var currentStatus = scenario.Statuses.FirstOrDefault(s => s.Id == scenario.Candidacy.CurrentStatusId);
        if (currentStatus == null || !currentStatus.IsFinal)
            return true; // only test when current status is final

        var (engine, _) = SetupEngine(scenario);

        var result = await engine.ExecuteTransitionAsync(
            scenario.Candidacy.Id,
            scenario.TargetStatusCode,
            "property test reason",
            userId: 1);

        return !result.IsSuccess;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<WorkflowScenario> Arbitrary() => WorkflowScenarioArb();
}
