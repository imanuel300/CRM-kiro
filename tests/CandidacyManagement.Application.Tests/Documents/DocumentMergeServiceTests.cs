using System.Linq.Expressions;
using CandidacyManagement.Application.Documents;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Documents;

public class DocumentMergeServiceTests
{
    private readonly Mock<IRepository<Document>> _documentRepoMock;
    private readonly DocumentMergeService _sut;

    public DocumentMergeServiceTests()
    {
        _documentRepoMock = new Mock<IRepository<Document>>();
        _sut = new DocumentMergeService(_documentRepoMock.Object);
    }

    [Fact]
    public async Task MergeAsync_WithTwoPdfs_ReturnsMergedResult()
    {
        var doc1 = new Document { Id = 1, FileName = "a.pdf", ContentType = "application/pdf", BlobUrl = "https://blob/a.pdf" };
        var doc2 = new Document { Id = 2, FileName = "b.pdf", ContentType = "application/pdf", BlobUrl = "https://blob/b.pdf" };

        _documentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(doc1);
        _documentRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(doc2);

        var command = new MergeDocumentsCommand(new List<int> { 1, 2 }, "combined.pdf");

        var result = await _sut.MergeAsync(command);

        result.MergedCount.Should().Be(2);
        result.FileName.Should().Be("combined.pdf");
        result.BlobUrl.Should().Contain("merged");
    }

    [Fact]
    public async Task MergeAsync_WithThreePdfs_ReturnsMergedCount3()
    {
        var docs = Enumerable.Range(1, 3).Select(i =>
            new Document { Id = i, FileName = $"doc{i}.pdf", ContentType = "application/pdf", BlobUrl = $"https://blob/doc{i}.pdf" }).ToList();

        foreach (var doc in docs)
            _documentRepoMock.Setup(r => r.GetByIdAsync(doc.Id, It.IsAny<CancellationToken>())).ReturnsAsync(doc);

        var command = new MergeDocumentsCommand(new List<int> { 1, 2, 3 });

        var result = await _sut.MergeAsync(command);

        result.MergedCount.Should().Be(3);
        result.FileName.Should().Be("merged.pdf"); // default
    }

    [Fact]
    public async Task MergeAsync_LessThanTwoDocuments_ThrowsValidationException()
    {
        var command = new MergeDocumentsCommand(new List<int> { 1 });

        var act = () => _sut.MergeAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task MergeAsync_EmptyList_ThrowsValidationException()
    {
        var command = new MergeDocumentsCommand(new List<int>());

        var act = () => _sut.MergeAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task MergeAsync_DocumentNotFound_ThrowsNotFoundException()
    {
        var doc1 = new Document { Id = 1, FileName = "a.pdf", ContentType = "application/pdf", BlobUrl = "https://blob/a.pdf" };
        _documentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(doc1);
        _documentRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>())).ReturnsAsync((Document?)null);

        var command = new MergeDocumentsCommand(new List<int> { 1, 999 });

        var act = () => _sut.MergeAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task MergeAsync_NonPdfDocument_ThrowsValidationException()
    {
        var doc1 = new Document { Id = 1, FileName = "a.pdf", ContentType = "application/pdf", BlobUrl = "https://blob/a.pdf" };
        var doc2 = new Document { Id = 2, FileName = "b.docx", ContentType = "application/vnd.openxmlformats", BlobUrl = "https://blob/b.docx" };

        _documentRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(doc1);
        _documentRepoMock.Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync(doc2);

        var command = new MergeDocumentsCommand(new List<int> { 1, 2 });

        var act = () => _sut.MergeAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task MergeAsync_BlobUrlContainsDocumentIds()
    {
        var doc1 = new Document { Id = 10, FileName = "a.pdf", ContentType = "application/pdf", BlobUrl = "https://blob/a.pdf" };
        var doc2 = new Document { Id = 20, FileName = "b.pdf", ContentType = "application/pdf", BlobUrl = "https://blob/b.pdf" };

        _documentRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>())).ReturnsAsync(doc1);
        _documentRepoMock.Setup(r => r.GetByIdAsync(20, It.IsAny<CancellationToken>())).ReturnsAsync(doc2);

        var command = new MergeDocumentsCommand(new List<int> { 10, 20 }, "output.pdf");

        var result = await _sut.MergeAsync(command);

        result.BlobUrl.Should().Contain("10-20");
    }
}
