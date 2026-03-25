using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Documents;

// NOTE: In production, add NuGet package PdfSharpCore (or iTextSharp) for actual PDF merging.
// <PackageReference Include="PdfSharpCore" Version="1.3.62" />
// The merge logic below simulates the merge by producing a combined blob URL.
// Replace the body of MergePdfBlobsAsync with real PdfSharpCore merge when the package is available.

public class DocumentMergeService : IDocumentMergeService
{
    private readonly IRepository<Document> _documentRepository;

    public DocumentMergeService(IRepository<Document> documentRepository)
    {
        _documentRepository = documentRepository;
    }

    public async Task<MergeDocumentsResult> MergeAsync(MergeDocumentsCommand command, CancellationToken cancellationToken = default)
    {
        if (command.DocumentIds == null || command.DocumentIds.Count < 2)
            throw new ValidationException("DocumentIds", "יש לספק לפחות שני מסמכים למיזוג");

        var documents = new List<Document>();
        foreach (var id in command.DocumentIds)
        {
            var doc = await _documentRepository.GetByIdAsync(id, cancellationToken)
                ?? throw new NotFoundException(nameof(Document), id);
            documents.Add(doc);
        }

        // Validate all documents are PDFs
        foreach (var doc in documents)
        {
            var ext = Path.GetExtension(doc.FileName)?.TrimStart('.').ToLowerInvariant() ?? string.Empty;
            if (ext != "pdf" && doc.ContentType != "application/pdf")
                throw new ValidationException("DocumentIds", $"מסמך '{doc.FileName}' (ID={doc.Id}) אינו קובץ PDF. ניתן למזג קבצי PDF בלבד");
        }

        var mergedBlobUrl = MergePdfBlobs(documents, command.OutputFileName);

        return new MergeDocumentsResult(
            mergedBlobUrl,
            command.OutputFileName,
            documents.Count);
    }

    /// <summary>
    /// Merges PDF blobs into a single PDF.
    /// TODO: Replace with actual PdfSharpCore merge implementation when the NuGet package is added.
    /// Example with PdfSharpCore:
    /// <code>
    /// using var outputDocument = new PdfSharp.Pdf.PdfDocument();
    /// foreach (var doc in documents)
    /// {
    ///     using var inputDocument = PdfSharp.Pdf.IO.PdfReader.Open(doc.BlobUrl, PdfSharp.Pdf.IO.PdfDocumentOpenMode.Import);
    ///     foreach (var page in inputDocument.Pages)
    ///         outputDocument.AddPage(page);
    /// }
    /// outputDocument.Save(outputStream);
    /// </code>
    /// </summary>
    private static string MergePdfBlobs(List<Document> documents, string outputFileName)
    {
        // Simulate merge: produce a deterministic blob URL from the source document IDs
        var ids = string.Join("-", documents.Select(d => d.Id));
        return $"https://blob.storage/merged/{ids}/{outputFileName}";
    }
}
