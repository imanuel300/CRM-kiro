using System.Linq.Expressions;
using CandidacyManagement.Application.Contacts;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.Contacts;

/// <summary>
/// Feature: unified-candidacy-management, Property 2: Contact Uniqueness by ID Number (ייחודיות תעודת זהות)
/// 
/// **Validates: Requirements 2.2, 2.7**
/// 
/// For any ID number, attempting to create a contact with an ID number that already exists
/// in the system results in a BusinessRuleViolationException. The total number of contacts
/// with that ID number remains exactly one.
/// </summary>
public class ContactIdUniquenessPropertyTests
{
    /// <summary>
    /// Data container for a generated contact creation scenario.
    /// </summary>
    public record ContactScenario(
        string IdNumber,
        string FirstName1,
        string LastName1,
        string FirstName2,
        string LastName2);

    /// <summary>
    /// Custom Arbitrary that generates valid contact creation scenarios:
    /// - A non-empty IdNumber (digits only, 5-9 chars to simulate Israeli ID numbers)
    /// - Two sets of first/last names for two creation attempts with the same IdNumber
    /// </summary>
    private static Arbitrary<ContactScenario> ContactScenarioArb()
    {
        var nonEmptyAlpha = Gen.Elements("אברהם", "שרה", "יצחק", "רבקה", "יעקב", "רחל", "משה", "דבורה", "דוד", "אסתר");
        var lastNames = Gen.Elements("כהן", "לוי", "מזרחי", "פרץ", "ביטון", "אברהמי", "דהן", "אוחיון", "גולן", "שלום");

        return Arb.From(
            from idLen in Gen.Choose(5, 9)
            from digits in Gen.ArrayOf(idLen, Gen.Choose(0, 9))
            let idNumber = string.Concat(digits)
            from firstName1 in nonEmptyAlpha
            from lastName1 in lastNames
            from firstName2 in nonEmptyAlpha
            from lastName2 in lastNames
            select new ContactScenario(idNumber, firstName1, lastName1, firstName2, lastName2));
    }

    private static (ContactService service, Mock<IRepository<Contact>> contactRepo, List<Contact> store) SetupService()
    {
        var contactRepoMock = new Mock<IRepository<Contact>>();
        var changeHistoryRepoMock = new Mock<IRepository<ContactChangeHistory>>();
        var customFieldValueRepoMock = new Mock<IRepository<ContactCustomFieldValue>>();
        var customFieldDefRepoMock = new Mock<IRepository<CustomFieldDefinition>>();
        var unitOfWorkMock = new Mock<IUnitOfWork>();

        // In-memory store to track created contacts
        var store = new List<Contact>();
        var nextId = 1;

        // FindAsync simulates lookup by predicate against the in-memory store
        contactRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .Returns((Expression<Func<Contact, bool>> predicate, CancellationToken _) =>
            {
                var compiled = predicate.Compile();
                var result = store.Where(compiled).ToList();
                return Task.FromResult<IEnumerable<Contact>>(result);
            });

        // AddAsync simulates adding to the in-memory store
        contactRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .Returns((Contact entity, CancellationToken _) =>
            {
                entity.Id = nextId++;
                store.Add(entity);
                return Task.FromResult(entity);
            });

        var service = new ContactService(
            contactRepoMock.Object,
            changeHistoryRepoMock.Object,
            customFieldValueRepoMock.Object,
            customFieldDefRepoMock.Object,
            unitOfWorkMock.Object);

        return (service, contactRepoMock, store);
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 2: Contact Uniqueness by ID Number
    /// **Validates: Requirements 2.2, 2.7**
    /// 
    /// For any valid IdNumber, creating a first contact succeeds, and attempting to create
    /// a second contact with the same IdNumber always throws BusinessRuleViolationException.
    /// After both attempts, exactly one contact with that IdNumber exists.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ContactIdUniquenessPropertyTests) })]
    public async Task<bool> DuplicateIdNumberAlwaysRejected(ContactScenario scenario)
    {
        var (service, _, store) = SetupService();

        // First creation should succeed
        var firstCommand = new CreateContactCommand(
            scenario.IdNumber,
            scenario.FirstName1,
            scenario.LastName1,
            null, null, null, null, null);

        var firstResult = await service.CreateAsync(firstCommand);
        firstResult.IdNumber.Should().Be(scenario.IdNumber);

        // Second creation with the same IdNumber should throw
        var secondCommand = new CreateContactCommand(
            scenario.IdNumber,
            scenario.FirstName2,
            scenario.LastName2,
            null, null, null, null, null);

        var threw = false;
        try
        {
            await service.CreateAsync(secondCommand);
        }
        catch (BusinessRuleViolationException)
        {
            threw = true;
        }

        // Verify: second attempt threw AND exactly one contact with this IdNumber exists
        var contactsWithId = store.Count(c => c.IdNumber == scenario.IdNumber);

        return threw && contactsWithId == 1;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<ContactScenario> Arbitrary() => ContactScenarioArb();
}
