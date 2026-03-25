using System.Linq.Expressions;
using CandidacyManagement.Application.Roles;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;
using FluentAssertions;
using Moq;

namespace CandidacyManagement.Application.Tests.Roles;

public class RoleServiceTests
{
    private readonly Mock<IRepository<Role>> _roleRepoMock;
    private readonly Mock<IRepository<Permission>> _permissionRepoMock;
    private readonly Mock<IRepository<UserRole>> _userRoleRepoMock;
    private readonly Mock<IRepository<AuditLogEntry>> _auditLogRepoMock;
    private readonly Mock<IRepository<OrganizationalUnit>> _orgUnitRepoMock;
    private readonly RoleService _sut;

    public RoleServiceTests()
    {
        _roleRepoMock = new Mock<IRepository<Role>>();
        _permissionRepoMock = new Mock<IRepository<Permission>>();
        _userRoleRepoMock = new Mock<IRepository<UserRole>>();
        _auditLogRepoMock = new Mock<IRepository<AuditLogEntry>>();
        _orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();

        _sut = new RoleService(
            _roleRepoMock.Object,
            _permissionRepoMock.Object,
            _userRoleRepoMock.Object,
            _auditLogRepoMock.Object,
            _orgUnitRepoMock.Object);
    }

    #region CreateRoleAsync

    [Fact]
    public async Task CreateRoleAsync_WithValidCommand_ReturnsRoleDto()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _roleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Role>());
        _roleRepoMock.Setup(r => r.AddAsync(It.IsAny<Role>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role e, CancellationToken _) => { e.Id = 10; return e; });
        _permissionRepoMock.Setup(r => r.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission e, CancellationToken _) => { e.Id = 100; return e; });

        var command = new CreateRoleCommand(
            Name: "מנהל יחידה",
            Description: "תפקיד ניהולי",
            OrgUnitId: 1,
            AllowCrossUnit: false,
            Permissions: new List<PermissionType> { PermissionType.View, PermissionType.Edit });

        var result = await _sut.CreateRoleAsync(command);

        result.Should().NotBeNull();
        result.Name.Should().Be("מנהל יחידה");
        result.OrgUnitId.Should().Be(1);
        result.AllowCrossUnit.Should().BeFalse();
        result.Permissions.Should().HaveCount(2);
        result.Permissions.Should().Contain(PermissionType.View);
        result.Permissions.Should().Contain(PermissionType.Edit);
    }

    [Fact]
    public async Task CreateRoleAsync_EmptyName_ThrowsValidationException()
    {
        var command = new CreateRoleCommand("", null, 1, false, new List<PermissionType>());

        var act = () => _sut.CreateRoleAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task CreateRoleAsync_NonExistentOrgUnit_ThrowsNotFoundException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((OrganizationalUnit?)null);

        var command = new CreateRoleCommand("תפקיד", null, 999, false, new List<PermissionType>());

        var act = () => _sut.CreateRoleAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task CreateRoleAsync_DuplicateName_ThrowsValidationException()
    {
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _roleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new Role { Id = 5, Name = "מנהל", OrgUnitId = 1 } });

        var command = new CreateRoleCommand("מנהל", null, 1, false, new List<PermissionType>());

        var act = () => _sut.CreateRoleAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region UpdateRoleAsync

    [Fact]
    public async Task UpdateRoleAsync_WithValidCommand_ReturnsUpdatedRole()
    {
        var existingRole = new Role { Id = 10, Name = "ישן", OrgUnitId = 1 };
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRole);
        _roleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Role, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Role>());
        _permissionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Permission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<Permission>());
        _permissionRepoMock.Setup(r => r.AddAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Permission e, CancellationToken _) => e);

        var command = new UpdateRoleCommand(10, "חדש", "תיאור חדש", true,
            new List<PermissionType> { PermissionType.View });

        var result = await _sut.UpdateRoleAsync(command);

        result.Name.Should().Be("חדש");
        result.Description.Should().Be("תיאור חדש");
        result.AllowCrossUnit.Should().BeTrue();
        result.Permissions.Should().ContainSingle().Which.Should().Be(PermissionType.View);
    }

    [Fact]
    public async Task UpdateRoleAsync_NonExistentRole_ThrowsNotFoundException()
    {
        _roleRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var command = new UpdateRoleCommand(999, "שם", null, false, new List<PermissionType>());

        var act = () => _sut.UpdateRoleAsync(command);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region DeleteRoleAsync

    [Fact]
    public async Task DeleteRoleAsync_ExistingRole_DeletesRoleAndRelated()
    {
        var role = new Role { Id = 10, OrgUnitId = 1 };
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);
        _permissionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Permission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new Permission { Id = 1, RoleId = 10 } });
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<UserRole>());

        await _sut.DeleteRoleAsync(10);

        _roleRepoMock.Verify(r => r.DeleteAsync(role, It.IsAny<CancellationToken>()), Times.Once);
        _permissionRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Permission>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRoleAsync_NonExistentRole_ThrowsNotFoundException()
    {
        _roleRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var act = () => _sut.DeleteRoleAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region AssignUserRoleAsync

    [Fact]
    public async Task AssignUserRoleAsync_ValidAssignment_ReturnsUserRoleDto()
    {
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 10, Name = "מנהל", OrgUnitId = 1 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<UserRole>());
        _userRoleRepoMock.Setup(r => r.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRole e, CancellationToken _) => { e.Id = 50; return e; });

        var command = new AssignUserRoleCommand(UserId: 5, RoleId: 10, OrgUnitId: 1);

        var result = await _sut.AssignUserRoleAsync(command);

        result.Should().NotBeNull();
        result.UserId.Should().Be(5);
        result.RoleId.Should().Be(10);
        result.RoleName.Should().Be("מנהל");
    }

    [Fact]
    public async Task AssignUserRoleAsync_DuplicateAssignment_ThrowsValidationException()
    {
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 10, Name = "מנהל", OrgUnitId = 1 });
        _orgUnitRepoMock.Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationalUnit { Id = 1 });
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new UserRole { Id = 50, UserId = 5, RoleId = 10, OrgUnitId = 1 } });

        var command = new AssignUserRoleCommand(UserId: 5, RoleId: 10, OrgUnitId: 1);

        var act = () => _sut.AssignUserRoleAsync(command);

        await act.Should().ThrowAsync<ValidationException>();
    }

    #endregion

    #region HasPermissionAsync

    [Fact]
    public async Task HasPermissionAsync_UserWithPermission_ReturnsTrue()
    {
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new UserRole { UserId = 5, RoleId = 10, OrgUnitId = 1 } });
        _permissionRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Permission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _sut.HasPermissionAsync(5, 1, PermissionType.View);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_UserWithoutPermission_ReturnsFalse()
    {
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new UserRole { UserId = 5, RoleId = 10, OrgUnitId = 1 } });
        _permissionRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Permission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _sut.HasPermissionAsync(5, 1, PermissionType.Delete);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_CrossUnitAccess_ReturnsTrueForOtherOrgUnit()
    {
        // משתמש משויך ליחידה 1 עם גישה חוצת-יחידות
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new UserRole { UserId = 5, RoleId = 10, OrgUnitId = 1 } });
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 10, AllowCrossUnit = true, OrgUnitId = 1 });
        _permissionRepoMock.Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Permission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // בדיקת גישה ליחידה 2 (שונה מהיחידה המשויכת)
        var result = await _sut.HasPermissionAsync(5, 2, PermissionType.View);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasPermissionAsync_NoCrossUnitAccess_ReturnsFalseForOtherOrgUnit()
    {
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new UserRole { UserId = 5, RoleId = 10, OrgUnitId = 1 } });
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 10, AllowCrossUnit = false, OrgUnitId = 1 });

        // בדיקת גישה ליחידה 2 ללא הרשאת חוצה-יחידות
        var result = await _sut.HasPermissionAsync(5, 2, PermissionType.View);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task HasPermissionAsync_NoRoles_ReturnsFalse()
    {
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Enumerable.Empty<UserRole>());

        var result = await _sut.HasPermissionAsync(5, 1, PermissionType.View);

        result.Should().BeFalse();
    }

    #endregion

    #region HasCrossUnitAccessAsync

    [Fact]
    public async Task HasCrossUnitAccessAsync_WithCrossUnitRole_ReturnsTrue()
    {
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new UserRole { UserId = 5, RoleId = 10, OrgUnitId = 1 } });
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 10, AllowCrossUnit = true });

        var result = await _sut.HasCrossUnitAccessAsync(5);

        result.Should().BeTrue();
    }

    [Fact]
    public async Task HasCrossUnitAccessAsync_WithoutCrossUnitRole_ReturnsFalse()
    {
        _userRoleRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new UserRole { UserId = 5, RoleId = 10, OrgUnitId = 1 } });
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 10, AllowCrossUnit = false });

        var result = await _sut.HasCrossUnitAccessAsync(5);

        result.Should().BeFalse();
    }

    #endregion

    #region LogActionAsync & GetAuditLogsAsync

    [Fact]
    public async Task LogActionAsync_CreatesAuditLogEntry()
    {
        _auditLogRepoMock.Setup(r => r.AddAsync(It.IsAny<AuditLogEntry>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((AuditLogEntry e, CancellationToken _) => e);

        var command = new CreateAuditLogCommand(
            UserId: 5, Action: "Create", EntityType: "Candidacy",
            EntityId: 100, OrgUnitId: 1, Details: "יצירת מועמדות חדשה");

        await _sut.LogActionAsync(command);

        _auditLogRepoMock.Verify(r => r.AddAsync(
            It.Is<AuditLogEntry>(e =>
                e.UserId == 5 &&
                e.Action == "Create" &&
                e.EntityType == "Candidacy" &&
                e.EntityId == 100),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAuditLogsAsync_ReturnsFilteredLogs()
    {
        var logs = new[]
        {
            new AuditLogEntry { Id = 1, UserId = 5, Action = "Create", EntityType = "Role", OrgUnitId = 1, Timestamp = DateTime.UtcNow },
            new AuditLogEntry { Id = 2, UserId = 5, Action = "Update", EntityType = "Role", OrgUnitId = 1, Timestamp = DateTime.UtcNow }
        };
        _auditLogRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<AuditLogEntry, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(logs);

        var query = new AuditLogQueryParams(UserId: 5, OrgUnitId: 1);
        var result = await _sut.GetAuditLogsAsync(query);

        result.Should().HaveCount(2);
        result.All(l => l.UserId == 5).Should().BeTrue();
    }

    #endregion

    #region GetRoleByIdAsync

    [Fact]
    public async Task GetRoleByIdAsync_ExistingRole_ReturnsRoleWithPermissions()
    {
        _roleRepoMock.Setup(r => r.GetByIdAsync(10, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Role { Id = 10, Name = "מנהל", OrgUnitId = 1 });
        _permissionRepoMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Permission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { new Permission { RoleId = 10, PermissionType = PermissionType.View } });

        var result = await _sut.GetRoleByIdAsync(10);

        result.Name.Should().Be("מנהל");
        result.Permissions.Should().ContainSingle().Which.Should().Be(PermissionType.View);
    }

    [Fact]
    public async Task GetRoleByIdAsync_NonExistent_ThrowsNotFoundException()
    {
        _roleRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        var act = () => _sut.GetRoleByIdAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion

    #region RemoveUserRoleAsync

    [Fact]
    public async Task RemoveUserRoleAsync_ExistingAssignment_Deletes()
    {
        var userRole = new UserRole { Id = 50, UserId = 5, RoleId = 10, OrgUnitId = 1 };
        _userRoleRepoMock.Setup(r => r.GetByIdAsync(50, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userRole);

        await _sut.RemoveUserRoleAsync(50);

        _userRoleRepoMock.Verify(r => r.DeleteAsync(userRole, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveUserRoleAsync_NonExistent_ThrowsNotFoundException()
    {
        _userRoleRepoMock.Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserRole?)null);

        var act = () => _sut.RemoveUserRoleAsync(999);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    #endregion
}
