using System.Linq.Expressions;
using CandidacyManagement.Application.Documents;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Documents;

public class DocumentServiceTests
{
    private readonly Mock<IRepository<Document>> _documentRepoMock;
    private readonly Mock<IRepository<RequiredDocument>> _requiredDocRepoMock;
    private readonly Mock<IRepository<Candidacy>> _candidacyRepoMock;
    private readonly Mock<IRepository<CallForCandidates>> _callRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly DocumentService _sut;

    public DocumentServiceTests()
    {
        _documentRepoMock = new Mock<IRepository<Document>>();
        _requiredDocRepoMock = new Mock<IRepository<RequiredDocument>>();
        _candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        _callRepoMock = new Mock<IRepository<CallForCandidates>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();

        _sut = new DocumentService(
            _documentRepoMock.Object,
            _requiredDocRepoMock.Object,
            _candidacyRepoMock.Object,
            _callRepoMock.Object,
            _orgUnitRepoMock.Object);
    }

    #region Upload

    [Fact]
    public async Task UploadAsync_WithValidCommand_ReturnsDocumentDto()
    {
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CallForCandidatesId = 20 };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _documentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Document, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Document>());
        _requiredDocRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RequiredDocument, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<RequiredDocument>());
        _documentRepoMock.Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document e, CancellationToken _) => { e.Id = 100; return e; });

        var command = new UploadDocumentCommand(1, "CV", "resume.pdf", "https://blob/resume.pdf", "application/pdf", 512000);

        var result = await _sut.UploadAsync(command);

        result.Should().NotBeNull();
        result.CandidacyId.Should().Be(1);
        result.DocumentType.Should().Be("CV");
        result.FileName.Should().Be("resume.pdf");
        result.Status.Should().Be("Uploaded");
        result.Version.Should().Be(1);
    }

    [Fact]
    public async Task UploadAsync_SecondVersion_IncrementsVersion()
    {
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CallForCandidatesId = 20 };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _documentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Document, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document> { new() { Id = 50, Version = 1, DocumentType = "CV", CandidacyId = 1 } });
        _requiredDocRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RequiredDocument, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<RequiredDocument>());
        _documentRepoMock.Setup(r => r.AddAsync(It.IsAny<Document>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document e, CancellationToken _) => { e.Id = 101; return e; });

        var command = new UploadDocumentCommand(1, "CV", "resume_v2.pdf", "https://blob/resume_v2.pdf", "application/pdf", 600000);

        var result = await _sut.UploadAsync(command);

        result.Version.Should().Be(2);
    }

    [Fact]
    public async Task UploadAsync_CandidacyNotFound_ThrowsNotFoundException()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Candidacy?)null);

        var command = new UploadDocumentCommand(999, "CV", "resume.pdf", "https://blob/resume.pdf", "application/pdf", 512000);

        var act = () => _sut.UploadAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UploadAsync_EmptyFileName_ThrowsValidationException()
    {
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CallForCandidatesId = 20 };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        var command = new UploadDocumentCommand(1, "CV", "", "https://blob/resume.pdf", "application/pdf", 512000);

        var act = () => _sut.UploadAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UploadAsync_InvalidFormat_ThrowsValidationException()
    {
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CallForCandidatesId = 20 };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _requiredDocRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RequiredDocument, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RequiredDocument>
            {
                new() { Id = 1, DocumentType = "CV", AllowedFormats = "pdf,docx", MaxSizeKB = 10240 }
            });

        var command = new UploadDocumentCommand(1, "CV", "resume.exe", "https://blob/resume.exe", "application/octet-stream", 512000);

        var act = () => _sut.UploadAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task UploadAsync_ExceedsMaxSize_ThrowsValidationException()
    {
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CallForCandidatesId = 20 };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);
        _requiredDocRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<RequiredDocument, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RequiredDocument>
            {
                new() { Id = 1, DocumentType = "CV", AllowedFormats = "pdf", MaxSizeKB = 100 }
            });

        // 200KB > 100KB limit
        var command = new UploadDocumentCommand(1, "CV", "resume.pdf", "https://blob/resume.pdf", "application/pdf", 200 * 1024);

        var act = () => _sut.UploadAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Review

    [Fact]
    public async Task ReviewAsync_Approve_UpdatesStatus()
    {
        var doc = new Document { Id = 1, CandidacyId = 1, Status = "Uploaded", DocumentType = "CV", FileName = "cv.pdf" };
        _documentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var command = new ReviewDocumentCommand(1, "Approved", 42);

        var result = await _sut.ReviewAsync(command);

        result.Status.Should().Be("Approved");
        result.ReviewedByUserId.Should().Be(42);
    }

    [Fact]
    public async Task ReviewAsync_Reject_UpdatesStatus()
    {
        var doc = new Document { Id = 2, CandidacyId = 1, Status = "Uploaded", DocumentType = "CV", FileName = "cv.pdf" };
        _documentRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var command = new ReviewDocumentCommand(2, "Rejected", 42);

        var result = await _sut.ReviewAsync(command);

        result.Status.Should().Be("Rejected");
    }

    [Fact]
    public async Task ReviewAsync_InvalidStatus_ThrowsValidationException()
    {
        var doc = new Document { Id = 1, CandidacyId = 1, Status = "Uploaded", DocumentType = "CV", FileName = "cv.pdf" };
        _documentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(doc);

        var command = new ReviewDocumentCommand(1, "InvalidStatus", 42);

        var act = () => _sut.ReviewAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task ReviewAsync_DocumentNotFound_ThrowsNotFoundException()
    {
        _documentRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Document?)null);

        var command = new ReviewDocumentCommand(999, "Approved", 42);

        var act = () => _sut.ReviewAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region Version History

    [Fact]
    public async Task GetVersionHistoryAsync_ReturnsOrderedVersions()
    {
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Candidacy { Id = 1 });
        _documentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Document, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>
            {
                new() { Id = 1, CandidacyId = 1, DocumentType = "CV", FileName = "v1.pdf", Version = 1, UploadedAt = DateTime.UtcNow.AddDays(-2) },
                new() { Id = 2, CandidacyId = 1, DocumentType = "CV", FileName = "v2.pdf", Version = 2, UploadedAt = DateTime.UtcNow.AddDays(-1) },
                new() { Id = 3, CandidacyId = 1, DocumentType = "CV", FileName = "v3.pdf", Version = 3, UploadedAt = DateTime.UtcNow }
            });

        var result = (await _sut.GetVersionHistoryAsync(1, "CV")).ToList();

        result.Should().HaveCount(3);
        result[0].Version.Should().Be(3);
        result[1].Version.Should().Be(2);
        result[2].Version.Should().Be(1);
    }

    #endregion

    #region Required Documents

    [Fact]
    public async Task CreateRequiredDocumentAsync_ForCall_ReturnsDto()
    {
        _callRepoMock.Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CallForCandidates { Id = 5 });
        _requiredDocRepoMock.Setup(r => r.AddAsync(It.IsAny<RequiredDocument>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RequiredDocument e, CancellationToken _) => { e.Id = 10; return e; });

        var command = new CreateRequiredDocumentCommand(5, null, "CV", true, "pdf,docx", 5120);

        var result = await _sut.CreateRequiredDocumentAsync(command);

        result.Should().NotBeNull();
        result.DocumentType.Should().Be("CV");
        result.IsRequired.Should().BeTrue();
        result.CallForCandidatesId.Should().Be(5);
    }

    [Fact]
    public async Task CreateRequiredDocumentAsync_NoScope_ThrowsValidationException()
    {
        var command = new CreateRequiredDocumentCommand(null, null, "CV", true, "pdf", 5120);

        var act = () => _sut.CreateRequiredDocumentAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region Completeness Check

    [Fact]
    public async Task CheckCompletenessAsync_AllRequiredPresent_ReturnsComplete()
    {
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CallForCandidatesId = 20 };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        // Required: CV (required), Letter (optional)
        _requiredDocRepoMock.SetupSequence(r => r.FindAsync(It.IsAny<Expression<Func<RequiredDocument, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RequiredDocument>
            {
                new() { Id = 1, DocumentType = "CV", IsRequired = true, CallForCandidatesId = 20 },
                new() { Id = 2, DocumentType = "Letter", IsRequired = false, CallForCandidatesId = 20 }
            })
            .ReturnsAsync(Enumerable.Empty<RequiredDocument>());

        // Uploaded: CV (approved)
        _documentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Document, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>
            {
                new() { Id = 10, CandidacyId = 1, DocumentType = "CV", Status = "Approved", Version = 1 }
            });

        var result = await _sut.CheckCompletenessAsync(1);

        result.IsComplete.Should().BeTrue();
        result.MissingDocuments.Should().ContainSingle(m => m.DocumentType == "Letter" && !m.IsRequired);
    }

    [Fact]
    public async Task CheckCompletenessAsync_RequiredMissing_ReturnsIncomplete()
    {
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CallForCandidatesId = 20 };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        _requiredDocRepoMock.SetupSequence(r => r.FindAsync(It.IsAny<Expression<Func<RequiredDocument, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RequiredDocument>
            {
                new() { Id = 1, DocumentType = "CV", IsRequired = true, CallForCandidatesId = 20 },
                new() { Id = 2, DocumentType = "Diploma", IsRequired = true, CallForCandidatesId = 20 }
            })
            .ReturnsAsync(Enumerable.Empty<RequiredDocument>());

        // No documents uploaded
        _documentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Document, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Document>());

        var result = await _sut.CheckCompletenessAsync(1);

        result.IsComplete.Should().BeFalse();
        result.MissingDocuments.Should().HaveCount(2);
        result.MissingDocuments.Should().Contain(m => m.DocumentType == "CV" && m.IsRequired);
        result.MissingDocuments.Should().Contain(m => m.DocumentType == "Diploma" && m.IsRequired);
    }

    [Fact]
    public async Task CheckCompletenessAsync_RejectedDocTreatedAsMissing()
    {
        var candidacy = new Candidacy { Id = 1, OrgUnitId = 10, CallForCandidatesId = 20 };
        _candidacyRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        _requiredDocRepoMock.SetupSequence(r => r.FindAsync(It.IsAny<Expression<Func<RequiredDocument, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<RequiredDocument>
            {
                new() { Id = 1, DocumentType = "CV", IsRequired = true, CallForCandidatesId = 20 }
            })
            .ReturnsAsync(Enumerable.Empty<RequiredDocument>());

        _documentRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Document, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Document>
            {
                new() { Id = 10, CandidacyId = 1, DocumentType = "CV", Status = "Rejected", Version = 1 }
            });

        var result = await _sut.CheckCompletenessAsync(1);

        result.IsComplete.Should().BeFalse();
        result.MissingDocuments.Should().ContainSingle(m => m.DocumentType == "CV" && m.IsRequired);
    }

    #endregion
}
