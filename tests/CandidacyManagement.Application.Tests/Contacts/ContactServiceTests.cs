using System.Linq.Expressions;
using CandidacyManagement.Application.Contacts;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Contacts;

public class ContactServiceTests
{
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<ContactChangeHistory>> _changeHistoryRepoMock;
    private readonly Mock<IRepository<ContactCustomFieldValue>> _customFieldValueRepoMock;
    private readonly Mock<IRepository<CustomFieldDefinition>> _customFieldDefRepoMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly ContactService _sut;

    public ContactServiceTests()
    {
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _changeHistoryRepoMock = new Mock<IRepository<ContactChangeHistory>>();
        _customFieldValueRepoMock = new Mock<IRepository<ContactCustomFieldValue>>();
        _customFieldDefRepoMock = new Mock<IRepository<CustomFieldDefinition>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new ContactService(
            _contactRepoMock.Object,
            _changeHistoryRepoMock.Object,
            _customFieldValueRepoMock.Object,
            _customFieldDefRepoMock.Object,
            _unitOfWorkMock.Object);
    }

    #region Create

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsDto()
    {
        var command = new CreateContactCommand("123456789", "ישראל", "ישראלי", new DateTime(1990, 1, 1), "זכר", "תל אביב", "050-1234567", "test@example.com");

        _contactRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Contact>());
        _contactRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact e, CancellationToken _) => e);

        var result = await _sut.CreateAsync(command);

        result.Should().NotBeNull();
        result.IdNumber.Should().Be("123456789");
        result.FirstName.Should().Be("ישראל");
        result.LastName.Should().Be("ישראלי");
        _contactRepoMock.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateIdNumber_ThrowsBusinessRuleViolation()
    {
        var command = new CreateContactCommand("123456789", "ישראל", "ישראלי", null, null, null, null, null);
        var existing = new Contact { Id = 5, IdNumber = "123456789", FirstName = "קיים", LastName = "קיים" };

        _contactRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existing });

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*123456789*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateAsync_WithEmptyIdNumber_ThrowsValidationException(string? idNumber)
    {
        var command = new CreateContactCommand(idNumber!, "שם", "משפחה", null, null, null, null, null);

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Theory]
    [InlineData("", "משפחה")]
    [InlineData("שם", "")]
    [InlineData("   ", "משפחה")]
    [InlineData("שם", "   ")]
    public async Task CreateAsync_WithEmptyName_ThrowsValidationException(string firstName, string lastName)
    {
        var command = new CreateContactCommand("123456789", firstName, lastName, null, null, null, null, null);

        _contactRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Contact>());

        var act = () => _sut.CreateAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsUpdatedDto()
    {
        var existing = new Contact { Id = 1, IdNumber = "123456789", FirstName = "ישן", LastName = "ישן", Phone = "050-1111111" };
        var command = new UpdateContactCommand(1, "חדש", "חדש", null, null, null, "050-2222222", null, 10);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _changeHistoryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ContactChangeHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContactChangeHistory e, CancellationToken _) => e);

        var result = await _sut.UpdateAsync(command);

        result.FirstName.Should().Be("חדש");
        result.LastName.Should().Be("חדש");
        result.Phone.Should().Be("050-2222222");

        // Should record 3 changes: FirstName, LastName, Phone
        _changeHistoryRepoMock.Verify(
            r => r.AddAsync(It.IsAny<ContactChangeHistory>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task UpdateAsync_WithNoChanges_DoesNotRecordHistory()
    {
        var existing = new Contact { Id = 1, IdNumber = "123456789", FirstName = "שם", LastName = "משפחה" };
        var command = new UpdateContactCommand(1, "שם", "משפחה", null, null, null, null, null, null);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.UpdateAsync(command);

        _changeHistoryRepoMock.Verify(
            r => r.AddAsync(It.IsAny<ContactChangeHistory>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        var command = new UpdateContactCommand(999, "שם", "משפחה", null, null, null, null, null, null);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.UpdateAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_AuditTrailRecordsCorrectValues()
    {
        var existing = new Contact { Id = 1, IdNumber = "123", FirstName = "ישן", LastName = "ישן" };
        var command = new UpdateContactCommand(1, "חדש", "ישן", null, null, null, null, null, 42);

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _changeHistoryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ContactChangeHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContactChangeHistory e, CancellationToken _) => e);

        await _sut.UpdateAsync(command);

        _changeHistoryRepoMock.Verify(r => r.AddAsync(
            It.Is<ContactChangeHistory>(h =>
                h.FieldName == "FirstName" &&
                h.OldValue == "ישן" &&
                h.NewValue == "חדש" &&
                h.ChangedByUserId == 42),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_ExistingContact_ReturnsDto()
    {
        var existing = new Contact { Id = 1, IdNumber = "123456789", FirstName = "ישראל", LastName = "ישראלי" };

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var result = await _sut.GetByIdAsync(1);

        result.Id.Should().Be(1);
        result.IdNumber.Should().Be("123456789");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        _contactRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.GetByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetByIdNumber

    [Fact]
    public async Task GetByIdNumberAsync_ExistingContact_ReturnsDto()
    {
        var existing = new Contact { Id = 1, IdNumber = "123456789", FirstName = "ישראל", LastName = "ישראלי" };

        _contactRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { existing });

        var result = await _sut.GetByIdNumberAsync("123456789");

        result.Should().NotBeNull();
        result!.IdNumber.Should().Be("123456789");
    }

    [Fact]
    public async Task GetByIdNumberAsync_NotFound_ReturnsNull()
    {
        _contactRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Contact>());

        var result = await _sut.GetByIdNumberAsync("999999999");

        result.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task GetByIdNumberAsync_EmptyInput_ReturnsNull(string? idNumber)
    {
        var result = await _sut.GetByIdNumberAsync(idNumber!);

        result.Should().BeNull();
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ExistingContact_CallsDelete()
    {
        var existing = new Contact { Id = 1, IdNumber = "123", FirstName = "שם", LastName = "משפחה" };

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await _sut.DeleteAsync(1);

        _contactRepoMock.Verify(r => r.DeleteAsync(existing, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        _contactRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.DeleteAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region ChangeHistory

    [Fact]
    public async Task GetChangeHistoryAsync_ReturnsOrderedHistory()
    {
        var contact = new Contact { Id = 1, IdNumber = "123", FirstName = "שם", LastName = "משפחה" };
        var history = new List<ContactChangeHistory>
        {
            new() { Id = 1, ContactId = 1, FieldName = "FirstName", OldValue = "א", NewValue = "ב", ChangedAt = DateTime.UtcNow.AddHours(-2) },
            new() { Id = 2, ContactId = 1, FieldName = "Phone", OldValue = null, NewValue = "050-1111111", ChangedAt = DateTime.UtcNow.AddHours(-1) },
            new() { Id = 3, ContactId = 1, FieldName = "Email", OldValue = "a@b.com", NewValue = "c@d.com", ChangedAt = DateTime.UtcNow }
        };

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        _changeHistoryRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ContactChangeHistory, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        var result = (await _sut.GetChangeHistoryAsync(1)).ToList();

        result.Should().HaveCount(3);
        result.First().FieldName.Should().Be("Email"); // Most recent first
    }

    [Fact]
    public async Task GetChangeHistoryAsync_ContactNotFound_ThrowsNotFoundException()
    {
        _contactRepoMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var act = () => _sut.GetChangeHistoryAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region CustomFields

    [Fact]
    public async Task GetCustomFieldsAsync_ReturnsFieldsWithValues()
    {
        var definitions = new List<CustomFieldDefinition>
        {
            new() { Id = 10, OrgUnitId = 1, EntityType = "Contact", FieldName = "השכלה", FieldType = "Text", SortOrder = 1 },
            new() { Id = 11, OrgUnitId = 1, EntityType = "Contact", FieldName = "ניסיון", FieldType = "Number", SortOrder = 2 }
        };
        var values = new List<ContactCustomFieldValue>
        {
            new() { Id = 100, ContactId = 1, CustomFieldDefinitionId = 10, OrgUnitId = 1, Value = "תואר ראשון" }
        };

        _customFieldDefRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<CustomFieldDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(definitions);
        _customFieldValueRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ContactCustomFieldValue, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(values);

        var result = (await _sut.GetCustomFieldsAsync(1, 1)).ToList();

        result.Should().HaveCount(2);
        result[0].FieldName.Should().Be("השכלה");
        result[0].Value.Should().Be("תואר ראשון");
        result[1].FieldName.Should().Be("ניסיון");
        result[1].Value.Should().BeNull();
    }

    [Fact]
    public async Task SetCustomFieldValueAsync_NewValue_CreatesRecord()
    {
        var contact = new Contact { Id = 1, IdNumber = "123", FirstName = "שם", LastName = "משפחה" };
        var definition = new CustomFieldDefinition { Id = 10, OrgUnitId = 1, EntityType = "Contact", FieldName = "השכלה", FieldType = "Text" };

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        _customFieldDefRepoMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);
        _customFieldValueRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ContactCustomFieldValue, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<ContactCustomFieldValue>());
        _customFieldValueRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ContactCustomFieldValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ContactCustomFieldValue e, CancellationToken _) => e);

        var command = new SetCustomFieldValueCommand(1, 1, 10, "תואר ראשון");
        await _sut.SetCustomFieldValueAsync(command);

        _customFieldValueRepoMock.Verify(r => r.AddAsync(
            It.Is<ContactCustomFieldValue>(v => v.Value == "תואר ראשון" && v.ContactId == 1),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetCustomFieldValueAsync_WrongOrgUnit_ThrowsBusinessRuleViolation()
    {
        var contact = new Contact { Id = 1, IdNumber = "123", FirstName = "שם", LastName = "משפחה" };
        var definition = new CustomFieldDefinition { Id = 10, OrgUnitId = 2, EntityType = "Contact", FieldName = "השכלה", FieldType = "Text" };

        _contactRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        _customFieldDefRepoMock
            .Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(definition);

        var command = new SetCustomFieldValueCommand(1, 1, 10, "ערך");
        var act = () => _sut.SetCustomFieldValueAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    #endregion
}
