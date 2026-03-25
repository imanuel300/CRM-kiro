using CandidacyManagement.Application.BusinessRules;
using CandidacyManagement.Application.BusinessRules.Evaluators;
using CandidacyManagement.Application.Calendar;
using CandidacyManagement.Application.CallsForCandidates;
using CandidacyManagement.Application.Candidacies;
using CandidacyManagement.Application.Committees;
using CandidacyManagement.Application.ConflictsOfInterest;
using CandidacyManagement.Application.Contacts;
using CandidacyManagement.Application.Dashboard;
using CandidacyManagement.Application.Documents;
using CandidacyManagement.Application.Exams;
using CandidacyManagement.Application.ExternalSubmissions;
using CandidacyManagement.Application.Interviews;
using CandidacyManagement.Application.Notifications;
using CandidacyManagement.Application.OrganizationalUnits;
using CandidacyManagement.Application.Reports;
using CandidacyManagement.Application.Roles;
using CandidacyManagement.Application.Security;
using CandidacyManagement.Application.OrgStructure;
using CandidacyManagement.Application.Quotas;
using CandidacyManagement.Application.Screening;
using CandidacyManagement.Application.Tenures;
using CandidacyManagement.Application.ThresholdChecks;
using CandidacyManagement.Application.Workflow;
using CandidacyManagement.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace CandidacyManagement.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddScoped<IOrganizationalUnitService, OrganizationalUnitService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<ICandidacyService, CandidacyService>();
        services.AddScoped<ICallForCandidatesService, CallForCandidatesService>();
        services.AddScoped<IDocumentService, DocumentService>();
        services.AddScoped<IDocumentMergeService, DocumentMergeService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IInterviewService, InterviewService>();
        services.AddScoped<ICommitteeService, CommitteeService>();
        services.AddScoped<ICalendarService, CalendarService>();
        services.AddScoped<IWorkflowEngine, WorkflowEngine>();
        services.AddScoped<IWorkflowConfigService, WorkflowConfigService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddScoped<IBulkNotificationService, BulkNotificationService>();
        services.AddScoped<IExternalSubmissionService, ExternalSubmissionService>();
        services.AddScoped<IApiCallLogService, ApiCallLogService>();
        services.AddScoped<IConflictOfInterestService, ConflictOfInterestService>();

        // Threshold Check Service
        services.AddScoped<IThresholdCheckService, ThresholdCheckService>();

        // Report Service
        services.AddScoped<IReportService, ReportService>();

        // Role & Authorization Service
        services.AddScoped<IRoleService, RoleService>();

        // Security Service
        services.AddScoped<ISecurityService, SecurityService>();

        // Dashboard Service
        services.AddScoped<IDashboardService, DashboardService>();

        // Tenure Service
        services.AddScoped<ITenureService, TenureService>();

        // Quota Service
        services.AddScoped<IQuotaService, QuotaService>();

        // Org Structure Service
        services.AddScoped<IOrgStructureService, OrgStructureService>();

        // Screening Orchestrator
        services.AddScoped<IScreeningOrchestrator, ScreeningOrchestrator>();

        // Business Rules Engine
        services.AddScoped<IBusinessRulesEngine, BusinessRulesEngine>();
        services.AddScoped<DuplicatePreventionEvaluator>();
        services.AddScoped<ThresholdCheckEvaluator>();
        services.AddScoped<IEnumerable<KeyValuePair<BusinessRuleType, IRuleEvaluator>>>(sp =>
            new List<KeyValuePair<BusinessRuleType, IRuleEvaluator>>
            {
                new(BusinessRuleType.DuplicatePrevention, sp.GetRequiredService<DuplicatePreventionEvaluator>()),
                new(BusinessRuleType.ThresholdCheck, sp.GetRequiredService<ThresholdCheckEvaluator>())
            });

        return services;
    }
}
