using System.Linq.Expressions;
using CandidacyManagement.Application.ExternalSubmissions;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.ExternalSubmissions;

public class ApiCallLogServiceTests
{
    private readonly Mock<IRepository<ApiCallLog>> _repoMock;
    private readonly ApiCallLogService _sut;

    public ApiCallLogServiceTests()
    {
        _repoMock = new Mock<IRepository<ApiCallLog>>();
        _sut = new ApiCallLogService(_repoMock.Object);
    }

    [Fact]
    public async Task LogAsync_SavesLogEntry()
    {
        var log = new ApiCallLog
        {
            ExternalSystemId = "external-portal",
            Endpoint = "/api/external/submissions",
            HttpMethod = "POST",
            ResponseStatusCode = 201,
            IsSuccess = true,
            DurationMs = 150,
            Timestamp = DateTime.UtcNow
        };

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ApiCallLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiCallLog l, CancellationToken _) => { l.Id = 1; return l; });

        await _sut.LogAsync(log);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<ApiCallLog>(l =>
                l.ExternalSystemId == "external-portal" &&
                l.Endpoint == "/api/external/submissions" &&
                l.IsSuccess),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LogAsync_SavesErrorDetails_WhenCallFails()
    {
        var log = new ApiCallLog
        {
            ExternalSystemId = "external-portal",
            Endpoint = "/api/external/submissions",
            HttpMethod = "POST",
            ResponseStatusCode = 400,
            IsSuccess = false,
            ErrorDetails = "שדות חובה חסרים",
            DurationMs = 50,
            Timestamp = DateTime.UtcNow
        };

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ApiCallLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiCallLog l, CancellationToken _) => { l.Id = 2; return l; });

        await _sut.LogAsync(log);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<ApiCallLog>(l =>
                !l.IsSuccess &&
                l.ErrorDetails == "שדות חובה חסרים"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBySystemIdAsync_ReturnsLogsForSystem()
    {
        var logs = new List<ApiCallLog>
        {
            new() { Id = 1, ExternalSystemId = "portal-a", Endpoint = "/api/external/submissions" },
            new() { Id = 2, ExternalSystemId = "portal-a", Endpoint = "/api/external/submissions/validate" }
        };

        _repoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<ApiCallLog, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        var result = await _sut.GetBySystemIdAsync("portal-a");

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(l => l.ExternalSystemId.Should().Be("portal-a"));
    }

    [Fact]
    public async Task LogAsync_RecordsIpAddress()
    {
        var log = new ApiCallLog
        {
            ExternalSystemId = "external-portal",
            Endpoint = "/api/external/submissions",
            HttpMethod = "POST",
            ResponseStatusCode = 201,
            IsSuccess = true,
            IpAddress = "192.168.1.100",
            DurationMs = 100,
            Timestamp = DateTime.UtcNow
        };

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ApiCallLog>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ApiCallLog l, CancellationToken _) => { l.Id = 3; return l; });

        await _sut.LogAsync(log);

        _repoMock.Verify(r => r.AddAsync(
            It.Is<ApiCallLog>(l => l.IpAddress == "192.168.1.100"),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
