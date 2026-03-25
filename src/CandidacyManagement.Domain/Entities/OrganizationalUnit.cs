using CandidacyManagement.Domain.Common;

namespace CandidacyManagement.Domain.Entities;

/// <summary>
/// יחידה ארגונית - גוף ארגוני המנהל תהליך מיון מועמדים
/// </summary>
public class OrganizationalUnit : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<CallForCandidates> CallsForCandidates { get; set; } = new List<CallForCandidates>();
    public ICollection<Candidacy> Candidacies { get; set; } = new List<Candidacy>();
    public ICollection<WorkflowDefinition> WorkflowDefinitions { get; set; } = new List<WorkflowDefinition>();
    public ICollection<StatusDefinition> StatusDefinitions { get; set; } = new List<StatusDefinition>();
    public ICollection<StatusTransition> StatusTransitions { get; set; } = new List<StatusTransition>();
    public ICollection<BusinessRule> BusinessRules { get; set; } = new List<BusinessRule>();
    public ICollection<CustomFieldDefinition> CustomFieldDefinitions { get; set; } = new List<CustomFieldDefinition>();
}
