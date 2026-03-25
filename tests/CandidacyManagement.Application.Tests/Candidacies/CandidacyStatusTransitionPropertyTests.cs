using System.Linq.Expressions;
using CandidacyManagement.Application.Candidacies;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.Candidacies;

/// <summary>
/// Feature: unified-candidacy-management, Property 3: שלמות מעברי סטטוס (Final Status Immutability)
/// 
/// **Validates: Requirements 3.3, 3.7**
/// 
/// For any candidacy that is in a final status (IsFinal=true), attempting to transition
/// to any other status always throws BusinessRuleViolationException. The candidacy's
/// status remains unchanged after the rejected transition attempt.
/// </summary>
public class CandidacyStatusTransitionPropertyTests
{
    /// <summary>
    /// Data container for a generated final-status transition scenario.
    /// </summary>
    public record FinalStatusTransitionScenario(
        int CandidacyId,
        int OrgUnitId,
        int FinalStatusId,
        string FinalStatusCode,
        int TargetStatusId,
        string TargetStatusCode,
        int UserId,
        string? Reason);

    /// <summary>
    /// Custom Arbitrary that generates valid final-status transition scenarios:
    /// - A candidacy with a final status
    /// - A random target status to attempt transitioning to
    /// </summary>
    private static Arbitrary<FinalStatusTransitionScenario> FinalStatusTransitionScenarioArb()
    {
        var statusCodes = Gen.Elements(
            "accepted", "rejected", "withdrawn", "disqualified", "completed",
            "submitted", "in_review", "exam_pending", "interview_pending", "committee_pending");

        var reasons = Gen.Elements<string?>(
            null, "בדיקה", "שינוי סטטוס", "עדכון", "תיקון");

        return Arb.From(
            from candidacyId in Gen.Choose(1, 10000)
            from orgUnitId in Gen.Choose(1, 100)
            from finalStatusId in Gen.Choose(1, 500)
            from finalCode in statusCodes
            from targetStatusId in Gen.Choose(501, 1000)
            from targetCode in statusCodes
            from userId in Gen.Choose(1, 1000)
            from reason in reasons
            select new FinalStatusTransitionScenario(
                candidacyId, orgUnitId, finalStatusId, finalCode,
                targetStatusId, targetCode, userId, reason));
    }

    private static (CandidacyService service,
        Mock<IRepository<Candidacy>> candidacyRepo,
        Mock<IRepository<StatusDefinition>> statusRepo,
        Mock<IRepository<StatusTransition>> transitionRepo,
        Mock<IRepository<CandidacyStatusHistory>> historyRepo,
        List<Candidacy> store) SetupService()
    {
        var candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        var contactRepoMock = new Mock<IRepository<Contact>>();
        var orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        var callRepoMock = new Mock<IRepository<CallForCandidates>>();
        var statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        var workflowRepoMock = new Mock<IRepository<WorkflowDefinition>>();
        var customFieldValueRepoMock = new Mock<IRepository<CandidacyCustomFieldValue>>();
        var customFieldDefRepoMock = new Mock<IRepository<CustomFieldDefinition>>();
        var transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        var historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();

        var store = new List<Candidacy>();

        var service = new CandidacyService(
            candidacyRepoMock.Object,
            contactRepoMock.Object,
            orgUnitRepoMock.Object,
            callRepoMock.Object,
            statusRepoMock.Object,
            workflowRepoMock.Object,
            customFieldValueRepoMock.Object,
            customFieldDefRepoMock.Object,
            transitionRepoMock.Object,
            historyRepoMock.Object);

        return (service, candidacyRepoMock, statusRepoMock, transitionRepoMock, historyRepoMock, store);
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 3: שלמות מעברי סטטוס
    /// **Validates: Requirements 3.3, 3.7**
    /// 
    /// For any candidacy in a final status, attempting to transition to any target status
    /// always throws BusinessRuleViolationException. The candidacy's status remains unchanged.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(CandidacyStatusTransitionPropertyTests) })]
    public async Task<bool> CandidacyInFinalStatusCannotTransition(FinalStatusTransitionScenario scenario)
    {
        var (service, candidacyRepo, statusRepo, transitionRepo, historyRepo, store) = SetupService();

        // Set up a candidacy that is in a final status
        var candidacy = new Candidacy
        {
            Id = scenario.CandidacyId,
            ContactId = 1,
            OrgUnitId = scenario.OrgUnitId,
            CallForCandidatesId = 1,
            CurrentStatusId = scenario.FinalStatusId,
            IsActive = false, // Final status means inactive
        };
        store.Add(candidacy);

        candidacyRepo
            .Setup(r => r.GetByIdAsync(scenario.CandidacyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        // The current status is final
        statusRepo
            .Setup(r => r.GetByIdAsync(scenario.FinalStatusId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StatusDefinition
            {
                Id = scenario.FinalStatusId,
                OrgUnitId = scenario.OrgUnitId,
                Code = scenario.FinalStatusCode,
                IsFinal = true,
                Category = CandidacyStatusCategory.Accepted
            });

        var command = new TransitionStatusCommand(
            scenario.CandidacyId,
            scenario.TargetStatusId,
            scenario.Reason,
            scenario.UserId);

        // Attempt transition - should throw
        var threw = false;
        try
        {
            await service.TransitionStatusAsync(command);
        }
        catch (BusinessRuleViolationException)
        {
            threw = true;
        }

        // Verify: transition was rejected AND status remains unchanged
        var statusUnchanged = candidacy.CurrentStatusId == scenario.FinalStatusId;

        return threw && statusUnchanged;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<FinalStatusTransitionScenario> Arbitrary() => FinalStatusTransitionScenarioArb();
}
