using System.Linq.Expressions;
using CandidacyManagement.Application.Candidacies;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.Candidacies;

/// <summary>
/// Feature: unified-candidacy-management, Property 4: מניעת מועמדות כפולה
/// 
/// **Validates: Requirements 3.5**
/// 
/// For any contact and call-for-candidates combination, there cannot be two active
/// candidacies. Creating a first candidacy succeeds, and attempting to create a second
/// candidacy for the same contact + call-for-candidates always throws
/// BusinessRuleViolationException.
/// </summary>
public class CandidacyDuplicatePreventionPropertyTests
{
    /// <summary>
    /// Data container for a generated duplicate candidacy scenario.
    /// </summary>
    public record DuplicateCandidacyScenario(
        int ContactId,
        int OrgUnitId,
        int CallForCandidatesId);

    /// <summary>
    /// Custom Arbitrary that generates valid duplicate candidacy scenarios
    /// with random contactId, orgUnitId, and callForCandidatesId combinations.
    /// </summary>
    private static Arbitrary<DuplicateCandidacyScenario> DuplicateCandidacyScenarioArb()
    {
        return Arb.From(
            from contactId in Gen.Choose(1, 10000)
            from orgUnitId in Gen.Choose(1, 100)
            from callForCandidatesId in Gen.Choose(1, 5000)
            select new DuplicateCandidacyScenario(contactId, orgUnitId, callForCandidatesId));
    }

    private static (CandidacyService service, List<Candidacy> store) SetupService(
        DuplicateCandidacyScenario scenario)
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
        var nextId = 1;

        // Referenced entities exist
        contactRepoMock
            .Setup(r => r.GetByIdAsync(scenario.ContactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Contact { Id = scenario.ContactId, IdNumber = "123456789", FirstName = "Test", LastName = "User" });

        orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(scenario.OrgUnitId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = scenario.OrgUnitId, Name = "Test Unit" });

        callRepoMock
            .Setup(r => r.GetByIdAsync(scenario.CallForCandidatesId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = scenario.CallForCandidatesId, Title = "Test Call", OrgUnitId = scenario.OrgUnitId });

        // ExistsAsync checks the in-memory store for duplicate active candidacy
        candidacyRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<Candidacy, bool>> predicate, CancellationToken _) =>
            {
                var compiled = predicate.Compile();
                return Task.FromResult(store.Any(compiled));
            });

        // AddAsync adds to the in-memory store
        candidacyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Candidacy>(), It.IsAny<CancellationToken>()))
            .Returns((Candidacy entity, CancellationToken _) =>
            {
                entity.Id = nextId++;
                store.Add(entity);
                return Task.FromResult(entity);
            });

        // No initial status or workflow (optional for this test)
        statusRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<StatusDefinition>());

        workflowRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<WorkflowDefinition>());

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
    /// Feature: unified-candidacy-management, Property 4: מניעת מועמדות כפולה
    /// **Validates: Requirements 3.5**
    /// 
    /// For any contactId, orgUnitId, and callForCandidatesId combination, creating a first
    /// candidacy succeeds, and attempting to create a second candidacy for the same contact
    /// + call-for-candidates always throws BusinessRuleViolationException. After both
    /// attempts, exactly one active candidacy exists for that combination.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(CandidacyDuplicatePreventionPropertyTests) })]
    public async Task<bool> DuplicateActiveCandidacyAlwaysRejected(DuplicateCandidacyScenario scenario)
    {
        var (service, store) = SetupService(scenario);

        var command = new CreateCandidacyCommand(
            scenario.ContactId,
            scenario.OrgUnitId,
            scenario.CallForCandidatesId);

        // First creation should succeed
        var firstResult = await service.CreateAsync(command);
        firstResult.ContactId.Should().Be(scenario.ContactId);
        firstResult.CallForCandidatesId.Should().Be(scenario.CallForCandidatesId);
        firstResult.IsActive.Should().BeTrue();

        // Second creation with same contact + call-for-candidates should throw
        var threw = false;
        try
        {
            await service.CreateAsync(command);
        }
        catch (BusinessRuleViolationException)
        {
            threw = true;
        }

        // Verify: second attempt threw AND exactly one active candidacy exists for this combination
        var activeCandidacies = store.Count(c =>
            c.ContactId == scenario.ContactId
            && c.CallForCandidatesId == scenario.CallForCandidatesId
            && c.IsActive);

        return threw && activeCandidacies == 1;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<DuplicateCandidacyScenario> Arbitrary() => DuplicateCandidacyScenarioArb();
}
