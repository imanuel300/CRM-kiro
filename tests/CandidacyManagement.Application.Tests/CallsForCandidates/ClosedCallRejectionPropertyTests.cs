using System.Linq.Expressions;
using CandidacyManagement.Application.Candidacies;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.CallsForCandidates;

/// <summary>
/// Feature: unified-candidacy-management, Property 5: חסימת הגשות לאחר סגירה (Closed Call Rejection)
/// 
/// **Validates: Requirements 4.3, 10.7**
/// 
/// For any call for candidates whose close date has passed, attempting to create a new
/// candidacy always throws BusinessRuleViolationException. No candidacy is persisted
/// in the store after the rejected attempt.
/// </summary>
public class ClosedCallRejectionPropertyTests
{
    /// <summary>
    /// Data container for a generated closed-call candidacy scenario.
    /// </summary>
    public record ClosedCallCandidacyScenario(
        int ContactId,
        int OrgUnitId,
        int CallForCandidatesId,
        DateTime CloseDate);

    /// <summary>
    /// Custom Arbitrary that generates valid closed-call scenarios:
    /// - Random entity IDs
    /// - A CloseDate that is always in the past (1 minute to 365 days ago)
    /// </summary>
    private static Arbitrary<ClosedCallCandidacyScenario> ClosedCallCandidacyScenarioArb()
    {
        return Arb.From(
            from contactId in Gen.Choose(1, 10000)
            from orgUnitId in Gen.Choose(1, 100)
            from callId in Gen.Choose(1, 5000)
            from minutesAgo in Gen.Choose(1, 525600) // 1 minute to ~365 days ago
            select new ClosedCallCandidacyScenario(
                contactId, orgUnitId, callId,
                DateTime.UtcNow.AddMinutes(-minutesAgo)));
    }

    private static (CandidacyService service, List<Candidacy> store) SetupService(
        ClosedCallCandidacyScenario scenario)
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

        // Contact exists
        contactRepoMock
            .Setup(r => r.GetByIdAsync(scenario.ContactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact
            {
                Id = scenario.ContactId,
                IdNumber = "123456789",
                FirstName = "Test",
                LastName = "User"
            });

        // OrgUnit exists
        orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(scenario.OrgUnitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit
            {
                Id = scenario.OrgUnitId,
                Name = "Test Unit"
            });

        // Call for candidates exists with a past close date
        callRepoMock
            .Setup(r => r.GetByIdAsync(scenario.CallForCandidatesId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates
            {
                Id = scenario.CallForCandidatesId,
                Title = "Closed Call",
                OrgUnitId = scenario.OrgUnitId,
                OpenDate = scenario.CloseDate.AddDays(-30),
                CloseDate = scenario.CloseDate,
                IsActive = true
            });

        // AddAsync tracks additions to the in-memory store
        candidacyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Candidacy>(), It.IsAny<CancellationToken>()))
            .Returns((Candidacy entity, CancellationToken _) =>
            {
                store.Add(entity);
                return Task.FromResult(entity);
            });

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

        return (service, store);
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 5: חסימת הגשות לאחר סגירה
    /// **Validates: Requirements 4.3, 10.7**
    /// 
    /// For any call for candidates whose close date has passed, attempting to create a new
    /// candidacy always throws BusinessRuleViolationException. No candidacy is added to
    /// the store after the rejected attempt.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ClosedCallRejectionPropertyTests) })]
    public async Task<bool> CandidacyCreationBlockedForClosedCall(ClosedCallCandidacyScenario scenario)
    {
        var (service, store) = SetupService(scenario);

        var command = new CreateCandidacyCommand(
            scenario.ContactId,
            scenario.OrgUnitId,
            scenario.CallForCandidatesId);

        // Attempt to create candidacy for a closed call - should throw
        var threw = false;
        try
        {
            await service.CreateAsync(command);
        }
        catch (BusinessRuleViolationException)
        {
            threw = true;
        }

        // Verify: creation was rejected AND no candidacy was persisted
        var noCandidacyPersisted = store.Count == 0;

        return threw && noCandidacyPersisted;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<ClosedCallCandidacyScenario> Arbitrary() => ClosedCallCandidacyScenarioArb();
}
