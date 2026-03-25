using System.Linq.Expressions;
using CandidacyManagement.Application.Roles;
using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using FsCheck;
using FsCheck.Xunit;
using Moq;

namespace CandidacyManagement.Application.Tests.Roles;

/// <summary>
/// Feature: unified-candidacy-management, Property 10: הגבלת גישה ליחידה (Org Unit Access Restriction)
/// 
/// **Validates: Requirements 12.3, 12.5**
/// 
/// For any user assigned to a specific organizational unit with a role that has
/// AllowCrossUnit = false, checking permission for a different organizational unit
/// always returns false, regardless of the permission type requested.
/// </summary>
public class RoleAccessRestrictionPropertyTests
{
    /// <summary>
    /// Data container for a generated cross-unit access denial scenario.
    /// </summary>
    public record CrossUnitDenialScenario(
        int UserId,
        int RoleId,
        int AssignedOrgUnitId,
        int TargetOrgUnitId,
        PermissionType Permission);

    /// <summary>
    /// Custom Arbitrary that generates scenarios where a user is assigned to one org unit
    /// with AllowCrossUnit = false, and the target org unit is always different from the assigned one.
    /// </summary>
    private static Arbitrary<CrossUnitDenialScenario> CrossUnitDenialScenarioArb()
    {
        return Arb.From(
            from userId in Gen.Choose(1, 10000)
            from roleId in Gen.Choose(1, 10000)
            from assignedOrgUnitId in Gen.Choose(1, 5000)
            from offset in Gen.Choose(1, 5000)
            let targetOrgUnitId = assignedOrgUnitId + offset
            from permission in Gen.Elements(
                PermissionType.View,
                PermissionType.Edit,
                PermissionType.Create,
                PermissionType.Delete,
                PermissionType.ChangeStatus,
                PermissionType.SendNotification)
            select new CrossUnitDenialScenario(
                userId, roleId, assignedOrgUnitId, targetOrgUnitId, permission));
    }

    /// <summary>
    /// Feature: unified-candidacy-management, Property 10: הגבלת גישה ליחידה
    /// **Validates: Requirements 12.3, 12.5**
    /// 
    /// For any user without cross-unit access, HasPermissionAsync always returns false
    /// when the target org unit differs from the user's assigned org unit.
    /// </summary>
    [Property(MaxTest = 100, Arbitrary = new[] { typeof(RoleAccessRestrictionPropertyTests) })]
    public async Task<bool> UserWithoutCrossUnitAccessDeniedForDifferentOrgUnit(CrossUnitDenialScenario scenario)
    {
        // Arrange
        var roleRepoMock = new Mock<IRepository<Role>>();
        var permissionRepoMock = new Mock<IRepository<Permission>>();
        var userRoleRepoMock = new Mock<IRepository<UserRole>>();
        var auditLogRepoMock = new Mock<IRepository<AuditLogEntry>>();
        var orgUnitRepoMock = new Mock<IRepository<OrganizationalUnit>>();

        // User is assigned to assignedOrgUnitId with a role that has AllowCrossUnit = false
        var userRole = new UserRole
        {
            Id = 1,
            UserId = scenario.UserId,
            RoleId = scenario.RoleId,
            OrgUnitId = scenario.AssignedOrgUnitId
        };

        userRoleRepoMock
            .Setup(r => r.FindAsync(It.IsAny<Expression<Func<UserRole, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new[] { userRole });

        // The role does NOT allow cross-unit access
        var role = new Role
        {
            Id = scenario.RoleId,
            Name = "TestRole",
            OrgUnitId = scenario.AssignedOrgUnitId,
            AllowCrossUnit = false
        };

        roleRepoMock
            .Setup(r => r.GetByIdAsync(scenario.RoleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        // The role has the requested permission (so denial is purely due to org unit restriction)
        permissionRepoMock
            .Setup(r => r.ExistsAsync(It.IsAny<Expression<Func<Permission, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var sut = new RoleService(
            roleRepoMock.Object,
            permissionRepoMock.Object,
            userRoleRepoMock.Object,
            auditLogRepoMock.Object,
            orgUnitRepoMock.Object);

        // Act: check permission for a DIFFERENT org unit
        var result = await sut.HasPermissionAsync(
            scenario.UserId, scenario.TargetOrgUnitId, scenario.Permission);

        // Assert: access must be denied
        return result == false;
    }

    // Expose the Arbitrary for FsCheck discovery
    public static Arbitrary<CrossUnitDenialScenario> Arbitrary() => CrossUnitDenialScenarioArb();
}
