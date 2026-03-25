using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.ExternalSubmissions;

/// <summary>
/// שירות קליטת מועמדויות ממערכת הגשה חיצונית
/// </summary>
public class ExternalSubmissionService : IExternalSubmissionService
{
    private readonly IRepository<Contact> _contactRepository;
    private readonly IRepository<Candidacy> _candidacyRepository;
    private readonly IRepository<CallForCandidates> _callRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;
    private readonly IRepository<StatusDefinition> _statusRepository;
    private readonly IRepository<WorkflowDefinition> _workflowRepository;
    private readonly IRepository<Document> _documentRepository;
    private readonly IRepository<ConflictOfInterest> _conflictRepository;
    private readonly IRepository<FamilyRelation> _familyRelationRepository;
    private readonly IRepository<RequiredDocument> _requiredDocumentRepository;

    public ExternalSubmissionService(
        IRepository<Contact> contactRepository,
        IRepository<Candidacy> candidacyRepository,
        IRepository<CallForCandidates> callRepository,
        IRepository<OrganizationalUnit> orgUnitRepository,
        IRepository<StatusDefinition> statusRepository,
        IRepository<WorkflowDefinition> workflowRepository,
        IRepository<Document> documentRepository,
        IRepository<ConflictOfInterest> conflictRepository,
        IRepository<FamilyRelation> familyRelationRepository,
        IRepository<RequiredDocument> requiredDocumentRepository)
    {
        _contactRepository = contactRepository;
        _candidacyRepository = candidacyRepository;
        _callRepository = callRepository;
        _orgUnitRepository = orgUnitRepository;
        _statusRepository = statusRepository;
        _workflowRepository = workflowRepository;
        _documentRepository = documentRepository;
        _conflictRepository = conflictRepository;
        _familyRelationRepository = familyRelationRepository;
        _requiredDocumentRepository = requiredDocumentRepository;
    }

    public async Task<SubmissionResult> SubmitAsync(ExternalSubmissionCommand command, CancellationToken cancellationToken = default)
    {
        // שלב 1: ולידציה מלאה
        var validation = await ValidateAsync(command, cancellationToken);
        if (!validation.IsValid)
            throw new ValidationException(validation.Errors);

        // שלב 2: אחזור קול קורא ויחידה ארגונית
        var call = (await _callRepository.GetByIdAsync(command.CallForCandidatesId, cancellationToken))!;
        var orgUnit = (await _orgUnitRepository.GetByIdAsync(call.OrgUnitId, cancellationToken))!;

        // שלב 3: יצירת/קישור איש קשר לפי תעודת זהות
        var contact = await FindOrCreateContactAsync(command, cancellationToken);

        // שלב 4: בדיקת מועמדות כפולה
        var duplicateExists = await _candidacyRepository.ExistsAsync(
            c => c.ContactId == contact.Id
                 && c.CallForCandidatesId == command.CallForCandidatesId
                 && c.IsActive,
            cancellationToken);

        if (duplicateExists)
            throw new BusinessRuleViolationException(
                "לא ניתן להגיש מועמדות כפולה - קיימת כבר מועמדות פעילה לאיש קשר זה בקול קורא זה");

        // שלב 5: קבלת סטטוס התחלתי
        var initialStatus = await GetInitialStatusAsync(call.OrgUnitId, cancellationToken);
        var workflowVersion = await GetActiveWorkflowVersionAsync(call.OrgUnitId, cancellationToken);

        // שלב 6: יצירת מועמדות
        var candidacy = new Candidacy
        {
            ContactId = contact.Id,
            OrgUnitId = call.OrgUnitId,
            CallForCandidatesId = command.CallForCandidatesId,
            CurrentStatusId = initialStatus?.Id,
            IsActive = true,
            SubmittedAt = DateTime.UtcNow,
            WorkflowDefinitionVersion = workflowVersion
        };

        await _candidacyRepository.AddAsync(candidacy, cancellationToken);

        // שלב 7: קליטת מסמכים מצורפים
        var documentIds = await ProcessDocumentsAsync(candidacy.Id, command.Documents, cancellationToken);

        // שלב 8: קליטת הצהרות ניגוד עניינים
        if (command.ConflictOfInterest != null)
        {
            await ProcessConflictOfInterestAsync(candidacy.Id, contact.Id, command.ConflictOfInterest, cancellationToken);
        }

        // שלב 9: קליטת הצהרות קרבה משפחתית
        if (command.FamilyRelations != null && command.FamilyRelations.Count > 0)
        {
            await ProcessFamilyRelationsAsync(candidacy.Id, contact.Id, command.FamilyRelations, cancellationToken);
        }

        return new SubmissionResult(
            candidacy.Id,
            contact.Id,
            "Submitted",
            candidacy.SubmittedAt ?? DateTime.UtcNow,
            documentIds);
    }

    public async Task<SubmissionValidationResult> ValidateAsync(ExternalSubmissionCommand command, CancellationToken cancellationToken = default)
    {
        var errors = new Dictionary<string, List<string>>();

        // ולידציה של שדות חובה בסיסיים
        if (string.IsNullOrWhiteSpace(command.IdNumber))
            AddError(errors, "IdNumber", "מספר תעודת זהות הוא שדה חובה");

        if (string.IsNullOrWhiteSpace(command.FirstName))
            AddError(errors, "FirstName", "שם פרטי הוא שדה חובה");

        if (string.IsNullOrWhiteSpace(command.LastName))
            AddError(errors, "LastName", "שם משפחה הוא שדה חובה");

        if (command.CallForCandidatesId <= 0)
            AddError(errors, "CallForCandidatesId", "מזהה קול קורא הוא שדה חובה");

        // ולידציה של קול קורא
        if (command.CallForCandidatesId > 0)
        {
            var call = await _callRepository.GetByIdAsync(command.CallForCandidatesId, cancellationToken);
            if (call == null)
            {
                AddError(errors, "CallForCandidatesId", "קול קורא לא נמצא");
            }
            else
            {
                // בדיקה שקול קורא לא נסגר
                if (call.CloseDate.HasValue && call.CloseDate.Value <= DateTime.UtcNow)
                    AddError(errors, "CallForCandidatesId", "לא ניתן להגיש מועמדות לקול קורא שתאריך הסגירה שלו חלף");

                if (!call.IsActive)
                    AddError(errors, "CallForCandidatesId", "קול קורא אינו פעיל");

                // ולידציה של יחידה ארגונית
                var orgUnit = await _orgUnitRepository.GetByIdAsync(call.OrgUnitId, cancellationToken);
                if (orgUnit == null)
                    AddError(errors, "OrgUnit", "יחידה ארגונית לא נמצאה");
                else if (!orgUnit.IsActive)
                    AddError(errors, "OrgUnit", "יחידה ארגונית אינה פעילה");
            }
        }

        // ולידציה של מסמכים מצורפים
        if (command.Documents != null)
        {
            for (int i = 0; i < command.Documents.Count; i++)
            {
                var doc = command.Documents[i];
                var prefix = $"Documents[{i}]";

                if (string.IsNullOrWhiteSpace(doc.DocumentType))
                    AddError(errors, $"{prefix}.DocumentType", "סוג מסמך הוא שדה חובה");

                if (string.IsNullOrWhiteSpace(doc.FileName))
                    AddError(errors, $"{prefix}.FileName", "שם קובץ הוא שדה חובה");

                if (string.IsNullOrWhiteSpace(doc.Base64Content))
                    AddError(errors, $"{prefix}.Base64Content", "תוכן הקובץ הוא שדה חובה");
                else if (!IsValidBase64(doc.Base64Content))
                    AddError(errors, $"{prefix}.Base64Content", "תוכן הקובץ אינו בפורמט Base64 תקין");
            }
        }

        // ולידציה של הצהרות קרבה משפחתית
        if (command.FamilyRelations != null)
        {
            for (int i = 0; i < command.FamilyRelations.Count; i++)
            {
                var rel = command.FamilyRelations[i];
                var prefix = $"FamilyRelations[{i}]";

                if (string.IsNullOrWhiteSpace(rel.RelationType))
                    AddError(errors, $"{prefix}.RelationType", "סוג קרבה הוא שדה חובה");

                if (string.IsNullOrWhiteSpace(rel.RelatedPersonName))
                    AddError(errors, $"{prefix}.RelatedPersonName", "שם הקרוב הוא שדה חובה");
            }
        }

        var result = errors.ToDictionary(
            kvp => kvp.Key,
            kvp => kvp.Value.ToArray());

        return new SubmissionValidationResult(result.Count == 0, result);
    }

    // --- Private helpers ---

    private async Task<Contact> FindOrCreateContactAsync(ExternalSubmissionCommand command, CancellationToken cancellationToken)
    {
        var idNumber = command.IdNumber.Trim();
        var existing = await _contactRepository.FindAsync(c => c.IdNumber == idNumber, cancellationToken);
        var contact = existing.FirstOrDefault();

        if (contact != null)
            return contact;

        // יצירת איש קשר חדש
        contact = new Contact
        {
            IdNumber = idNumber,
            FirstName = command.FirstName.Trim(),
            LastName = command.LastName.Trim(),
            Email = command.Email,
            Phone = command.Phone,
            DateOfBirth = command.DateOfBirth,
            Gender = command.Gender,
            Address = command.Address
        };

        await _contactRepository.AddAsync(contact, cancellationToken);
        return contact;
    }

    private async Task<List<int>> ProcessDocumentsAsync(
        int candidacyId,
        IReadOnlyList<AttachedDocumentDto>? documents,
        CancellationToken cancellationToken)
    {
        var documentIds = new List<int>();
        if (documents == null || documents.Count == 0)
            return documentIds;

        foreach (var doc in documents)
        {
            var bytes = Convert.FromBase64String(doc.Base64Content);

            var entity = new Document
            {
                CandidacyId = candidacyId,
                DocumentType = doc.DocumentType,
                FileName = doc.FileName,
                ContentType = doc.ContentType,
                SizeBytes = bytes.Length,
                BlobUrl = $"external/{candidacyId}/{doc.FileName}", // placeholder - בפועל יועלה ל-Blob Storage
                Status = "Uploaded",
                Version = 1,
                UploadedAt = DateTime.UtcNow
            };

            await _documentRepository.AddAsync(entity, cancellationToken);
            documentIds.Add(entity.Id);
        }

        return documentIds;
    }

    private async Task ProcessConflictOfInterestAsync(
        int candidacyId,
        int contactId,
        ConflictOfInterestDeclarationDto declaration,
        CancellationToken cancellationToken)
    {
        var entity = new ConflictOfInterest
        {
            CandidacyId = candidacyId,
            ContactId = contactId,
            QuestionnaireResponses = declaration.QuestionnaireResponses,
            HasConflict = declaration.HasConflict,
            RequiresManualReview = declaration.HasConflict // סימון לבדיקה ידנית אם יש ניגוד
        };

        await _conflictRepository.AddAsync(entity, cancellationToken);
    }

    private async Task ProcessFamilyRelationsAsync(
        int candidacyId,
        int contactId,
        IReadOnlyList<FamilyRelationDeclarationDto> relations,
        CancellationToken cancellationToken)
    {
        foreach (var rel in relations)
        {
            var entity = new FamilyRelation
            {
                CandidacyId = candidacyId,
                ContactId = contactId,
                RelationType = rel.RelationType,
                RelatedPersonName = rel.RelatedPersonName,
                RelatedPersonRole = rel.RelatedPersonRole,
                RequiresManualReview = true // כל הצהרת קרבה דורשת בדיקה ידנית
            };

            await _familyRelationRepository.AddAsync(entity, cancellationToken);
        }
    }

    private async Task<StatusDefinition?> GetInitialStatusAsync(int orgUnitId, CancellationToken cancellationToken)
    {
        var statuses = await _statusRepository.FindAsync(
            s => s.OrgUnitId == orgUnitId && s.IsInitial, cancellationToken);
        return statuses.FirstOrDefault();
    }

    private async Task<int?> GetActiveWorkflowVersionAsync(int orgUnitId, CancellationToken cancellationToken)
    {
        var workflows = await _workflowRepository.FindAsync(
            w => w.OrgUnitId == orgUnitId && w.IsActive, cancellationToken);
        return workflows.OrderByDescending(w => w.Version).FirstOrDefault()?.Version;
    }

    private static void AddError(Dictionary<string, List<string>> errors, string field, string message)
    {
        if (!errors.ContainsKey(field))
            errors[field] = new List<string>();
        errors[field].Add(message);
    }

    private static bool IsValidBase64(string value)
    {
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
}
