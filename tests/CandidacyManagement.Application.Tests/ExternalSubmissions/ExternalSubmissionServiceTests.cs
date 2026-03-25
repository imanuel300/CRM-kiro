using System.Linq.Expressions;
using CandidacyManagement.Application.ExternalSubmissions;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.ExternalSubmissions;

public class ExternalSubmissionServiceTests
{
    private readonly Mock<IRepository<Contact>> _contactRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<CallForCandidates>> _callRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly Mock<IRepository<StatusDefinition>> _statusRepoMock;
    private readonly Mock<IRepository<WorkflowDefinition>> _workflowRepoMock;
    private readonly Mock<IRepository<Document>> _documentRepoMock;
    private readonly Mock<IRepository<ConflictOfInterest>> _conflictRepoMock;
    private readonly Mock<IRepository<FamilyRelation>> _familyRelationRepoMock;
    private readonly Mock<IRepository<RequiredDocument>> _requiredDocRepoMock;
    private readonly ExternalSubmissionService _sut;

    public ExternalSubmissionServiceTests()
    {
        _contactRepoMock = new Mock<IRepository<Contact>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _callRepoMock = new Mock<IRepository<CallForCandidates>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        _statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        _workflowRepoMock = new Mock<IRepository<WorkflowDefinition>>();
        _documentRepoMock = new Mock<IRepository<Document>>();
        _conflictRepoMock = new Mock<IRepository<ConflictOfInterest>>();
        _familyRelationRepoMock = new Mock<IRepository<FamilyRelation>>();
        _requiredDocRepoMock = new Mock<IRepository<RequiredDocument>>();

        _sut = new ExternalSubmissionService(
            _contactRepoMock.Object,
            _candidacyRepoMock.Object,
            _callRepoMock.Object,
            _orgUnitRepoMock.Object,
            _statusRepoMock.Object,
            _workflowRepoMock.Object,
            _documentRepoMock.Object,
            _conflictRepoMock.Object,
            _familyRelationRepoMock.Object,
            _requiredDocRepoMock.Object);
    }

    #region SubmitAsync - Success

    [Fact]
    public async Task SubmitAsync_WithValidCommand_CreatesContactAndCandidacy()
    {
        var command = CreateValidCommand();
        SetupValidDependencies();

        var result = await _sut.SubmitAsync(command);

        result.Should().NotBeNull();
        result.Status.Should().Be("Submitted");
        result.SubmittedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));

        _contactRepoMock.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Once);
        _candidacyRepoMock.Verify(r => r.AddAsync(It.IsAny<Candidacy>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_WithExistingContact_LinksToExistingContact()
    {
        var command = CreateValidCommand();
        SetupValidDependencies();

        var existingContact = new Contact { Id = 42, IdNumber = "123456789", FirstName = "ישראל", LastName = "ישראלי" };
        _contactRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contact> { existingContact });

        var result = await _sut.SubmitAsync(command);

        result.ContactId.Should().Be(42);
        _contactRepoMock.Verify(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SubmitAsync_WithDocuments_CreatesDocumentRecords()
    {
        var command = CreateValidCommand() with
        {
            Documents = new List<AttachedDocumentDto>
            {
                new("CV", "cv.pdf", "application/pdf", Convert.ToBase64String(new byte[] { 1, 2, 3 })),
                new("Certificate", "cert.pdf", "application/pdf", Convert.ToBase64String(new byte[] { 4, 5, 6 }))
            }
        };
        SetupValidDependencies();

        var result = await _sut.SubmitAsync(command);

        _documentRepoMock.Verify(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task SubmitAsync_WithConflictOfInterest_CreatesConflictRecord()
    {
        var command = CreateValidCommand() with
        {
            ConflictOfInterest = new ConflictOfInterestDeclarationDto("שאלה 1: כן", true)
        };
        SetupValidDependencies();

        await _sut.SubmitAsync(command);

        _conflictRepoMock.Verify(r => r.AddAsync(
            It.Is<ConflictOfInterest>(c => c.HasConflict && c.RequiresManualReview),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SubmitAsync_WithFamilyRelations_CreatesFamilyRelationRecords()
    {
        var command = CreateValidCommand() with
        {
            FamilyRelations = new List<FamilyRelationDeclarationDto>
            {
                new("אח", "יוסי ישראלי", "שופט"),
                new("דוד", "משה ישראלי", null)
            }
        };
        SetupValidDependencies();

        await _sut.SubmitAsync(command);

        _familyRelationRepoMock.Verify(r => r.AddAsync(
            It.Is<FamilyRelation>(f => f.RequiresManualReview),
            It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    #endregion

    #region SubmitAsync - Validation Failures

    [Fact]
    public async Task SubmitAsync_MissingIdNumber_ThrowsValidationException()
    {
        var command = CreateValidCommand() with { IdNumber = "" };
        SetupValidDependencies();

        var act = () => _sut.SubmitAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAsync_MissingFirstName_ThrowsValidationException()
    {
        var command = CreateValidCommand() with { FirstName = "" };
        SetupValidDependencies();

        var act = () => _sut.SubmitAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAsync_MissingLastName_ThrowsValidationException()
    {
        var command = CreateValidCommand() with { LastName = "" };
        SetupValidDependencies();

        var act = () => _sut.SubmitAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAsync_ClosedCall_ThrowsValidationException()
    {
        var command = CreateValidCommand();
        SetupValidDependencies();

        _callRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates
            {
                Id = 1, OrgUnitId = 1, Title = "קול קורא",
                OpenDate = DateTime.UtcNow.AddDays(-30),
                CloseDate = DateTime.UtcNow.AddDays(-1), // סגור
                IsActive = true
            });

        var act = () => _sut.SubmitAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAsync_NonExistentCall_ThrowsValidationException()
    {
        var command = CreateValidCommand();
        SetupValidDependencies();

        _callRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        var act = () => _sut.SubmitAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task SubmitAsync_DuplicateCandidacy_ThrowsBusinessRuleViolation()
    {
        var command = CreateValidCommand();
        SetupValidDependencies();

        _candidacyRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var act = () => _sut.SubmitAsync(command);

        await act.Should().ThrowAsync<BusinessRuleViolationException>()
            .WithMessage("*מועמדות כפולה*");
    }

    #endregion

    #region ValidateAsync

    [Fact]
    public async Task ValidateAsync_ValidCommand_ReturnsIsValidTrue()
    {
        var command = CreateValidCommand();
        SetupValidDependencies();

        var result = await _sut.ValidateAsync(command);

        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task ValidateAsync_MissingMultipleFields_ReturnsAllErrors()
    {
        var command = new ExternalSubmissionCommand(
            "", "", "", null, null, null, null, null,
            0, null, null, null);

        var result = await _sut.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("IdNumber");
        result.Errors.Should().ContainKey("FirstName");
        result.Errors.Should().ContainKey("LastName");
        result.Errors.Should().ContainKey("CallForCandidatesId");
    }

    [Fact]
    public async Task ValidateAsync_InvalidBase64Document_ReturnsDocumentError()
    {
        var command = CreateValidCommand() with
        {
            Documents = new List<AttachedDocumentDto>
            {
                new("CV", "cv.pdf", "application/pdf", "not-valid-base64!!!")
            }
        };
        SetupValidDependencies();

        var result = await _sut.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("Documents[0].Base64Content");
    }

    [Fact]
    public async Task ValidateAsync_FamilyRelationMissingType_ReturnsError()
    {
        var command = CreateValidCommand() with
        {
            FamilyRelations = new List<FamilyRelationDeclarationDto>
            {
                new("", "יוסי", null)
            }
        };
        SetupValidDependencies();

        var result = await _sut.ValidateAsync(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainKey("FamilyRelations[0].RelationType");
    }

    #endregion

    #region Helpers

    private static ExternalSubmissionCommand CreateValidCommand() =>
        new(
            IdNumber: "123456789",
            FirstName: "ישראל",
            LastName: "ישראלי",
            Email: "test@example.com",
            Phone: "050-1234567",
            DateOfBirth: new DateTime(1990, 1, 1),
            Gender: "זכר",
            Address: "תל אביב",
            CallForCandidatesId: 1,
            Documents: null,
            ConflictOfInterest: null,
            FamilyRelations: null);

    private void SetupValidDependencies()
    {
        _callRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates
            {
                Id = 1, OrgUnitId = 1, Title = "קול קורא",
                OpenDate = DateTime.UtcNow.AddDays(-10),
                CloseDate = DateTime.UtcNow.AddDays(30),
                IsActive = true
            });

        _orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "יחידה", IsActive = true });

        _contactRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<Contact, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Contact>());

        _contactRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact c, CancellationToken _) => { c.Id = 1; return c; });

        _candidacyRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Candidacy, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _candidacyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Candidacy>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy c, CancellationToken _) => { c.Id = 100; return c; });

        _statusRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StatusDefinition>
            {
                new() { Id = 10, OrgUnitId = 1, Code = "Submitted", DisplayName = "הוגשה", IsInitial = true }
            });

        _workflowRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<WorkflowDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<WorkflowDefinition>
            {
                new() { Id = 1, OrgUnitId = 1, Version = 1, IsActive = true }
            });

        _documentRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document d, CancellationToken _) => { d.Id = 200; return d; });
    }

    #endregion
}
