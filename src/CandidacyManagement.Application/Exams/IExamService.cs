namespace CandidacyManagement.Application.Exams;

public interface IExamService
{
    // Exam CRUD
    Task<ExamDto> CreateAsync(CreateExamCommand command, CancellationToken cancellationToken = default);
    Task<ExamDto> UpdateAsync(UpdateExamCommand command, CancellationToken cancellationToken = default);
    Task<ExamDto> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExamDto>> ListAsync(ExamQueryParams query, CancellationToken cancellationToken = default);
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);

    // Scores
    Task<ExamScoreDto> SubmitScoreAsync(SubmitScoreCommand command, CancellationToken cancellationToken = default);
    Task<ExamScoreDto> GetScoreAsync(int examId, int candidacyId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ExamScoreDto>> GetScoresByExamAsync(int examId, CancellationToken cancellationToken = default);

    // Appeals
    Task<ExamScoreDto> SubmitAppealAsync(SubmitAppealCommand command, CancellationToken cancellationToken = default);
}
