using System.Linq.Expressions;
using CandidacyManagement.Application.ExternalSubmissions;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using FluentAssertions;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.ExternalSubmissions;

/// <summary>
/// Feature: unified-candidacy-management, Property 7: ולידצית קליטה (Submission Validation)
/// 
/// **Validates: Requirements 10.2, 10.3**
/// 
/// For any randomly generated ExternalSubmissionCommand that passes validation
/// (ValidateAsync returns IsValid=true), the command must have:
/// - Non-empty IdNumber
/// - Non-empty FirstName
/// - Non-empty LastName
/// - Valid CallForCandidatesId (> 0)
/// - If documents are present, each has non-empty DocumentType, FileName, and valid Base64Content
/// - If family relations are present, each has non-empty RelationType and RelatedPersonName
/// </summary>
public class ExternalSubmissionValidationPropertyTests
{
    /// <summary>
    /// Data container for a randomly generated external submission command.
    /// </summary>
    public record SubmissionScenario(
        string IdNumber,
        string FirstName,
        string LastName,
        string? Email,
        string? Phone,
        int CallForCandidatesId,
        List<AttachedDocumentDto>? Documents,
        List<FamilyRelationDeclarationDto>? FamilyRelations);

    /// <summary>
    /// Custom Arbitrary that generates a wide range of ExternalSubmissionCommand inputs,
    /// including both valid and invalid combinations, to test the validation boundary.
    /// </summary>
    private static Arbitrary<SubmissionScenario> SubmissionScenarioArb()
    {
        var names = Gen.Elements("ישראל", "שרה", "משה", "רחל", "דוד", "אסתר", "", " ", null!);
        var nonEmptyNames = Gen.Elements("ישראל", "שרה", "משה", "רחל", "דוד", "אסתר");
        var idNumbers = Gen.Elements("123456789", "987654321", "111222333", "", " ", null!);
        var emails = Gen.Elements<string?>("test@example.com", null, "user@mail.co.il");
        var phones = Gen.Elements<string?>("050-1234567", null, "03-9876543");
        var callIds = Gen.Choose(-1, 10);

        var docTypes = Gen.Elements("CV", "Certificate", "Diploma", "", " ", null!);
        var fileNames = Gen.Elements("file.pdf", "doc.docx", "", null!);
        var base64Values = Gen.Elements(
            Convert.ToBase64String(new byte[] { 1, 2, 3 }),
            Convert.ToBase64String(new byte[] { 10, 20, 30 }),
            "not-valid-base64!!!",
            "", null!);

        var docGen = from docType in docTypes
                     from fileName in fileNames
                     from base64 in base64Values
                     select new AttachedDocumentDto(docType ?? "", fileName ?? "", "application/pdf", base64 ?? "");

        var docsGen = Gen.OneOf(
            Gen.Constant<List<AttachedDocumentDto>?>(null),
            Gen.ListOf(docGen).Select(l => (List<AttachedDocumentDto>?)l.ToList()));

        var relationTypes = Gen.Elements("אח", "אחות", "דוד", "הורה", "", " ", null!);
        var relatedNames = Gen.Elements("יוסי כהן", "מרים לוי", "", null!);
        var relatedRoles = Gen.Elements<string?>("שופט", "פקיד", null);

        var relGen = from relType in relationTypes
                     from relName in relatedNames
                     from relRole in relatedRoles
                     select new FamilyRelationDeclarationDto(relType ?? "", relName ?? "", relRole);

        var relsGen = Gen.OneOf(
            Gen.Constant<List<FamilyRelationDeclarationDto>?>(null),
            Gen.ListOf(relGen).Select(l => (List<FamilyRelationDeclarationDto>?)l.ToList()));

        return Arb.From(
            from idNumber in idNumbers
            from firstName in names
            from lastName in names
            from email in emails
            from phone in phones
            from callId in callIds
            from docs in docsGen
            from rels in relsGen
            select new SubmissionScenario(
                idNumber ?? "",
                firstName ?? "",
                lastName ?? "",
                email,
                phone,
                callId,
                docs,
                rels));
    }

    private static ExternalSubmissionService SetupService(int validCallId)
    {
        var contactRepoMock = new Mock<IRepository<Contact>>();
        var candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        var callRepoMock = new Mock<IRepository<CallForCandidates>>();
        var orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();
        var statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        var workflowRepoMock = new Mock<IRepository<WorkflowDefinition>>();
        var documentRepoMock = new Mock<IRepository<Document>>();
        var conflictRepoMock = new Mock<IRepository<ConflictOfInterest>>();
        var familyRelationRepoMock = new Mock<IRepository<FamilyRelation>>();
        var requiredDocRepoMock = new Mock<IRepository<RequiredDocument>>();

        // Setup call repository: valid call IDs > 0 return an active, open call
        callRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id > 0 && id <= 10), It.IsAny<CancellationToken>()))
            .ReturnsAsync((int id, CancellationToken _) => new CallForCandidates
            {
                Id = id,
                OrgUnitId = 1,
                Title = "קול קורא",
                OpenDate = DateTime.UtcNow.AddDays(-10),
                CloseDate = DateTime.UtcNow.AddDays(30),
                IsActive = true
            });

        // Invalid call IDs return null
        callRepoMock
            .Setup(r => r.GetByIdAsync(It.Is<int>(id => id <= 0), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CallForCandidates?)null);

        // Org unit is always active
        orgUnitRepoMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1, Name = "יחידה", IsActive = true });

        return new ExternalSubmissionService(
            contactRepoMock.Object,
            candidacyRepoMock.Object,
            callRepoMock.Object,
            orgUnitRepoMock.Object,
            statusRepoMock.Object,
            workflowRepoMock.Object,
            documentRepoMock.Object,
            conflictRepoMock.Object,
            familyRelationRepoMock.Object,
            requiredDocRepoMock.Object);
    }

    private static bool IsValidBase64(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;
        try
        {
            Convert.FromBase64String(value);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 7: ולידצית קליטה
    /// **Validates: Requirements 10.2, 10.3**
    /// 
    /// For any randomly generated ExternalSubmissionCommand, if ValidateAsync returns
    /// IsValid=true, then the command must have all required fields valid:
    /// - Non-empty IdNumber, FirstName, LastName
    /// - CallForCandidatesId > 0
    /// - If documents present, each has non-empty DocumentType, FileName, and valid Base64Content
    /// - If family relations present, each has non-empty RelationType and RelatedPersonName
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ExternalSubmissionValidationPropertyTests) })]
    public async Task<bool> ValidSubmissionAlwaysHasAllRequiredFieldsValid(SubmissionScenario scenario)
    {
        var service = SetupService(scenario.CallForCandidatesId);

        var command = new ExternalSubmissionCommand(
            scenario.IdNumber,
            scenario.FirstName,
            scenario.LastName,
            scenario.Email,
            scenario.Phone,
            null, // DateOfBirth
            null, // Gender
            null, // Address
            scenario.CallForCandidatesId,
            scenario.Documents,
            null, // ConflictOfInterest
            scenario.FamilyRelations);

        var result = await service.ValidateAsync(command);

        // If validation says it's NOT valid, the property trivially holds (we only care about valid ones)
        if (!result.IsValid)
            return true;

        // If validation says it IS valid, verify all required fields are indeed valid
        var hasIdNumber = !string.IsNullOrWhiteSpace(scenario.IdNumber);
        var hasFirstName = !string.IsNullOrWhiteSpace(scenario.FirstName);
        var hasLastName = !string.IsNullOrWhiteSpace(scenario.LastName);
        var hasValidCallId = scenario.CallForCandidatesId > 0;

        var documentsValid = true;
        if (scenario.Documents != null)
        {
            foreach (var doc in scenario.Documents)
            {
                if (string.IsNullOrWhiteSpace(doc.DocumentType) ||
                    string.IsNullOrWhiteSpace(doc.FileName) ||
                    !IsValidBase64(doc.Base64Content))
                {
                    documentsValid = false;
                    break;
                }
            }
        }

        var familyRelationsValid = true;
        if (scenario.FamilyRelations != null)
        {
            foreach (var rel in scenario.FamilyRelations)
            {
                if (string.IsNullOrWhiteSpace(rel.RelationType) ||
                    string.IsNullOrWhiteSpace(rel.RelatedPersonName))
                {
                    familyRelationsValid = false;
                    break;
                }
            }
        }

        return hasIdNumber && hasFirstName && hasLastName && hasValidCallId
               && documentsValid && familyRelationsValid;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<SubmissionScenario> Arbitrary() => SubmissionScenarioArb();
}
