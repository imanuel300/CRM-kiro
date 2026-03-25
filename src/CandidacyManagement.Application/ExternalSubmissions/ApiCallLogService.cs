using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;

namespace CandidacyManagement.Application.ExternalSubmissions;

/// <summary>
/// שירות תיעוד קריאות API חיצוני - שומר כל קריאה ביומן
/// </summary>
public class ApiCallLogService : IApiCallLogService
{
    private readonly IRepository<ApiCallLog> _repository;

    public ApiCallLogService(IRepository<ApiCallLog> repository)
    {
        _repository = repository;
    }

    public async Task LogAsync(ApiCallLog log, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(log, cancellationToken);
    }

    public async Task<IEnumerable<ApiCallLog>> GetBySystemIdAsync(
        string externalSystemId, CancellationToken cancellationToken = default)
    {
        return await _repository.FindAsync(
            l => l.ExternalSystemId == externalSystemId, cancellationToken);
    }
}
