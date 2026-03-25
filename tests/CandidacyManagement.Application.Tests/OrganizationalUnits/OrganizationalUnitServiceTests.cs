using System.Linq.Expressions;
using CandidacyManagement.Application.OrganizationalUnits;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.OrganizationalUnits;

public class OrganizationalUnitServiceTests
{
    private readonly Mock<IRepository<OrganizationalUnit>> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly OrganizationalUnitService _sut;

    public OrganizationalUnitServiceTests()
    {
        _repositoryMock = new Mock<IRepository<OrganizationalUnit>>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _sut = new OrganizationalUnitService(_repositoryMock.Object, _unitOfWorkMock.Object);
    }

    #region Create

    [Fact]
    public async Task CreateAsync_WithValidCommand_ReturnsDto()
    {
        // Arrange
        var command = new CreateOrgUnitCommand("יחידת מבחן", "תיאור", "test@example.com", "050-1234567");

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<OrganizationalUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<OrganizationalUnit>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit entity, CancellationToken _) => entity);

        // Act
        var result = await _sut.CreateAsync(command);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("יחידת מבחן");
        result.Description.Should().Be("תיאור");
        result.ContactEmail.Should().Be("test@example.com");
        result.ContactPhone.Should().Be("050-1234567");
        result.IsActive.Should().BeTrue();

        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<OrganizationalUnit>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task CreateAsync_WithEmptyName_ThrowsValidationException(string? name)
    {
        // Arrange
        var command = new CreateOrgUnitCommand(name!, null, null, null);

        // Act
        var act = () => _sut.CreateAsync(command);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateAsync_WithDuplicateName_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var command = new CreateOrgUnitCommand("קיימת", null, null, null);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<OrganizationalUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.CreateAsync(command);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdateAsync_WithValidCommand_ReturnsUpdatedDto()
    {
        // Arrange
        var existing = new OrganizationalUnit { Id = 1, Name = "ישנה", IsActive = true };
        var command = new UpdateOrgUnitCommand(1, "חדשה", "תיאור חדש", "new@example.com", "050-9999999");

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<OrganizationalUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        // Act
        var result = await _sut.UpdateAsync(command);

        // Assert
        result.Name.Should().Be("חדשה");
        result.Description.Should().Be("תיאור חדש");
        result.ContactEmail.Should().Be("new@example.com");

        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<OrganizationalUnit>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateOrgUnitCommand(999, "שם", null, null, null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        // Act
        var act = () => _sut.UpdateAsync(command);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WithDuplicateName_ThrowsBusinessRuleViolationException()
    {
        // Arrange
        var existing = new OrganizationalUnit { Id = 1, Name = "ישנה", IsActive = true };
        var command = new UpdateOrgUnitCommand(1, "כפולה", null, null, null);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        _repositoryMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<OrganizationalUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = () => _sut.UpdateAsync(command);

        // Assert
        await act.Should().ThrowAsync<BusinessRuleViolationException>();
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeleteAsync_ExistingUnit_SetsIsActiveFalse()
    {
        // Arrange
        var existing = new OrganizationalUnit { Id = 1, Name = "למחיקה", IsActive = true };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        await _sut.DeleteAsync(1);

        // Assert
        existing.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(
            It.Is<OrganizationalUnit>(ou => !ou.IsActive),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        // Act
        var act = () => _sut.DeleteAsync(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetById

    [Fact]
    public async Task GetByIdAsync_ExistingUnit_ReturnsDto()
    {
        // Arrange
        var existing = new OrganizationalUnit
        {
            Id = 1, Name = "יחידה", Description = "תיאור",
            ContactEmail = "a@b.com", ContactPhone = "050-1111111", IsActive = true
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        // Act
        var result = await _sut.GetByIdAsync(1);

        // Assert
        result.Id.Should().Be(1);
        result.Name.Should().Be("יחידה");
        result.Description.Should().Be("תיאור");
    }

    [Fact]
    public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        // Act
        var act = () => _sut.GetByIdAsync(999);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region GetAll

    [Fact]
    public async Task GetAllAsync_ReturnsOnlyActiveUnits()
    {
        // Arrange
        var units = new List<OrganizationalUnit>
        {
            new() { Id = 1, Name = "פעילה1", IsActive = true },
            new() { Id = 2, Name = "פעילה2", IsActive = true }
        };

        _repositoryMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrganizationalUnit, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(units);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().AllSatisfy(dto => dto.IsActive.Should().BeTrue());

        _repositoryMock.Verify(
            r => r.FindAsync(It.IsAny<Expression<Func<OrganizationalUnit, bool>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    #endregion
}
