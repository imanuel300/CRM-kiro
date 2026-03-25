namespace CandidacyManagement.Application.Exams;

public record ExamDto(
    int Id,
    int OrgUnitId,
    int CallForCandidatesId,
    string Name,
    DateTime ExamDate,
    string? Location,
    decimal MaxScore,
    decimal? PassingScore,
    int? FirstExaminerId,
    int? SecondExaminerId,
    DateTime? AppealDeadline);

public record ExamScoreDto(
    int Id,
    int ExamId,
    int CandidacyId,
    decimal? FirstExaminerScore,
    decimal? SecondExaminerScore,
    decimal? FinalScore,
    string? ScoreFormula,
    bool? PassedThreshold,
    bool IsAppealed,
    decimal? AppealScore,
    DateTime? ScoredAt);

public record CreateExamCommand(
    int OrgUnitId,
    int CallForCandidatesId,
    string Name,
    DateTime ExamDate,
    string? Location,
    decimal MaxScore,
    decimal? PassingScore,
    int? FirstExaminerId,
    int? SecondExaminerId,
    DateTime? AppealDeadline);

public record UpdateExamCommand(
    int Id,
    string Name,
    DateTime ExamDate,
    string? Location,
    decimal MaxScore,
    decimal? PassingScore,
    int? FirstExaminerId,
    int? SecondExaminerId,
    DateTime? AppealDeadline);

public record SubmitScoreCommand(
    int ExamId,
    int CandidacyId,
    decimal? FirstExaminerScore,
    decimal? SecondExaminerScore);

public record SubmitAppealCommand(
    int ExamId,
    int CandidacyId,
    decimal AppealScore,
    string? Reason);

public record ExamQueryParams(
    int? OrgUnitId = null,
    int? CallForCandidatesId = null);
