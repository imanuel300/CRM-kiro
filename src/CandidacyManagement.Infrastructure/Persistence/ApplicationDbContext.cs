using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CandidacyManagement.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IUnitOfWork
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<WorkflowDefinition> WorkflowDefinitions => Set<WorkflowDefinition>();
    public DbSet<StatusDefinition> StatusDefinitions => Set<StatusDefinition>();
    public DbSet<SubStatusDefinition> SubStatusDefinitions => Set<SubStatusDefinition>();
    public DbSet<StatusTransition> StatusTransitions => Set<StatusTransition>();
    public DbSet<CandidacyStatusHistory> CandidacyStatusHistories => Set<CandidacyStatusHistory>();
    public DbSet<BusinessRule> BusinessRules => Set<BusinessRule>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<ContactChangeHistory> ContactChangeHistories => Set<ContactChangeHistory>();
    public DbSet<CustomFieldDefinition> CustomFieldDefinitions => Set<CustomFieldDefinition>();
    public DbSet<ContactCustomFieldValue> ContactCustomFieldValues => Set<ContactCustomFieldValue>();
    public DbSet<Candidacy> Candidacies => Set<Candidacy>();
    public DbSet<CandidacyCustomFieldValue> CandidacyCustomFieldValues => Set<CandidacyCustomFieldValue>();
    public DbSet<CallForCandidates> CallsForCandidates => Set<CallForCandidates>();
    public DbSet<ThresholdCondition> ThresholdConditions => Set<ThresholdCondition>();
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<OrganizationalUnit> OrganizationalUnits => Set<OrganizationalUnit>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = DateTime.UtcNow;
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = DateTime.UtcNow;
                    break;
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
