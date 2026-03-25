using System.Linq.Expressions;
using CandidacyManagement.Application.ThresholdChecks;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.ThresholdChecks;

/// <summary>
/// Feature: unified-candidacy-management, Property 9: עקביות בדיקת סף (Threshold Check Consistency)
/// 
/// **Validates: Requirements 15.2, 15.3**
/// 
/// For any candidacy that fails an automatic threshold condition, when a "failed_threshold"
/// status and a valid transition to it exist, the candidacy status is always updated to
/// that failed status according to the business rule. The candidacy becomes inactive
/// when the failed status is final.
/// </summary>
public class ThresholdCheckConsistencyPropertyTests
{
    /// <summary>
    /// Data container for a generated threshold check scenario where the candidacy
    /// fails an automatic condition.
    /// </summary>
    public record FailedThresholdScenario(
        int CandidacyId,
        int ContactId,
        int OrgUnitId,
        int CallForCandidatesId,
        int CurrentStatusId,
        int FailedStatusId,
        bool FailedStatusIsFinal,
        ConditionType ConditionType,
        string FieldName,
        string Operator,
        string RequiredValue,
        int CandidateAge,
        string? CandidateFieldValue);

    /// <summary>
    /// Custom Arbitrary that generates scenarios where a candidacy will fail
    /// an automatic threshold condition (Age, Score, or Education).
    /// The generator ensures the candidate's actual value does NOT meet the condition.
    /// </summary>
    private static Arbitrary<FailedThresholdScenario> FailedThresholdScenarioArb()
    {
        var ageFailGen =
            from requiredAge in Gen.Choose(25, 60)
            from candidateAge in Gen.Choose(18, requiredAge - 1)
            select (ConditionType: ConditionType.Age, FieldName: "Age", Operator: ">=",
                RequiredValue: requiredAge.ToString(), CandidateAge: candidateAge, CandidateFieldValue: (string?)null);

        var scoreFailGen =
            from requiredScore in Gen.Choose(50, 100)
            from candidateScore in Gen.Choose(0, requiredScore - 1)
            select (ConditionType: ConditionType.Score, FieldName: "Score", Operator: ">=",
                RequiredValue: requiredScore.ToString(), CandidateAge: 30, CandidateFieldValue: (string?)candidateScore.ToString());

        var educationFailGen =
            from required in Gen.Elements("תואר ראשון", "תואר שני", "תואר שלישי")
            from actual in Gen.Elements("תיכונית", "הנדסאי", "טכנאי")
            select (ConditionType: ConditionType.Education, FieldName: "Education", Operator: "==",
                RequiredValue: required, CandidateAge: 30, CandidateFieldValue: (string?)actual);

        var conditionGen = Gen.OneOf(ageFailGen, scoreFailGen, educationFailGen);

        return Arb.From(
            from candidacyId in Gen.Choose(1, 10000)
            from contactId in Gen.Choose(1, 10000)
            from orgUnitId in Gen.Choose(1, 100)
            from callId in Gen.Choose(1, 1000)
            from currentStatusId in Gen.Choose(1, 500)
            from failedStatusId in Gen.Choose(501, 1000)
            from failedIsFinal in Arb.Generate<bool>()
            from cond in conditionGen
            select new FailedThresholdScenario(
                candidacyId, contactId, orgUnitId, callId,
                currentStatusId, failedStatusId, failedIsFinal,
                cond.ConditionType, cond.FieldName, cond.Operator,
                cond.RequiredValue, cond.CandidateAge, cond.CandidateFieldValue));
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 9: עקביות בדיקת סף
    /// **Validates: Requirements 15.2, 15.3**
    /// 
    /// For any candidacy that fails an automatic threshold condition, when a
    /// "failed_threshold" status exists and a valid transition is defined,
    /// the candidacy status is updated to the failed status. If the failed
    /// status is final, the candidacy becomes inactive.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(ThresholdCheckConsistencyPropertyTests) })]
    public async Task<bool> FailedAutomaticThresholdUpdatesStatusAccordingToBusinessRule(FailedThresholdScenario scenario)
    {
        // Arrange: candidacy with an active status
        var candidacy = new Candidacy
        {
            Id = scenario.CandidacyId,
            ContactId = scenario.ContactId,
            OrgUnitId = scenario.OrgUnitId,
            CallForCandidatesId = scenario.CallForCandidatesId,
            CurrentStatusId = scenario.CurrentStatusId,
            IsActive = true
        };

        var contact = new Contact
        {
            Id = scenario.ContactId,
            DateOfBirth = DateTime.UtcNow.AddYears(-scenario.CandidateAge)
        };

        var candidacyRepoMock = new Mock<IRepository<Candidacy>>();
        var contactRepoMock = new Mock<IRepository<Contact>>();
        var conditionRepoMock = new Mock<IRepository<ThresholdCondition>>();
        var resultRepoMock = new Mock<IRepository<ThresholdCheckResult>>();
        var statusRepoMock = new Mock<IRepository<StatusDefinition>>();
        var transitionRepoMock = new Mock<IRepository<StatusTransition>>();
        var historyRepoMock = new Mock<IRepository<CandidacyStatusHistory>>();
        var customFieldRepoMock = new Mock<IRepository<CandidacyCustomFieldValue>>();

        candidacyRepoMock
            .Setup(r => r.GetByIdAsync(scenario.CandidacyId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(candidacy);

        contactRepoMock
            .Setup(r => r.GetByIdAsync(scenario.ContactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);

        // Arrange: single automatic condition that the candidate will fail
        var condition = new ThresholdCondition
        {
            Id = 1,
            CallForCandidatesId = scenario.CallForCandidatesId,
            FieldName = scenario.FieldName,
            Operator = scenario.Operator,
            Value = scenario.RequiredValue,
            IsAutomatic = true,
            ConditionType = scenario.ConditionType
        };

        conditionRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCondition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { condition });

        // For Score/Education conditions, provide the candidate's field value
        if (scenario.CandidateFieldValue != null)
        {
            customFieldRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<CandidacyCustomFieldValue, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new CandidacyCustomFieldValue { Id = 1, CandidacyId = scenario.CandidacyId, Value = scenario.CandidateFieldValue } });
        }
        else
        {
            customFieldRepoMock
                .Setup(r => r.FindAsync(It.IsAny<Expression<Func<CandidacyCustomFieldValue, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Array.Empty<CandidacyCustomFieldValue>());
        }

        // Result repo: no existing results, store new ones
        resultRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ThresholdCheckResult, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<ThresholdCheckResult>());
        resultRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ThresholdCheckResult>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ThresholdCheckResult e, CancellationToken _) => { e.Id = 1; return e; });

        // Arrange: "failed_threshold" status exists with a valid transition
        var failedStatus = new StatusDefinition
        {
            Id = scenario.FailedStatusId,
            OrgUnitId = scenario.OrgUnitId,
            Code = "failed_threshold",
            IsFinal = scenario.FailedStatusIsFinal
        };

        statusRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusDefinition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { failedStatus });

        var transition = new StatusTransition
        {
            Id = 1,
            OrgUnitId = scenario.OrgUnitId,
            FromStatusId = scenario.CurrentStatusId,
            ToStatusId = scenario.FailedStatusId
        };

        transitionRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<StatusTransition, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { transition });

        historyRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CandidacyStatusHistory>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((CandidacyStatusHistory h, CancellationToken _) => { h.Id = 1; return h; });

        var sut = new ThresholdCheckService(
            candidacyRepoMock.Object,
            contactRepoMock.Object,
            conditionRepoMock.Object,
            resultRepoMock.Object,
            statusRepoMock.Object,
            transitionRepoMock.Object,
            historyRepoMock.Object,
            customFieldRepoMock.Object);

        // Act
        var result = await sut.CheckAllAsync(scenario.CandidacyId);

        // Assert: the check should report failure
        var checkFailed = !result.AllPassed;

        // Assert: candidacy status was updated to the failed status
        var statusUpdated = candidacy.CurrentStatusId == scenario.FailedStatusId;

        // Assert: if failed status is final, candidacy should be inactive
        var inactiveCorrect = !scenario.FailedStatusIsFinal || !candidacy.IsActive;

        return checkFailed && statusUpdated && inactiveCorrect;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<FailedThresholdScenario> Arbitrary() => FailedThresholdScenarioArb();
}
