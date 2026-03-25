using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Roles;

/// <summary>
/// ממשק שירות ניהול תפקידים והרשאות
/// </summary>
public interface IRoleService
{
    // --- ניהול תפקידים ---
    Task<RoleDto> CreateRoleAsync(CreateRoleCommand command, CancellationToken cancellationToken = default);
    Task<RoleDto> UpdateRoleAsync(UpdateRoleCommand command, CancellationToken cancellationToken = default);
    Task<RoleDto> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IEnumerable<RoleDto>> GetRolesByOrgUnitAsync(int orgUnitId, CancellationToken cancellationToken = default);
    Task DeleteRoleAsync(int id, CancellationToken cancellationToken = default);

    // --- שיוך משתמשים לתפקידים ---
    Task<UserRoleDto> AssignUserRoleAsync(AssignUserRoleCommand command, CancellationToken cancellationToken = default);
    Task RemoveUserRoleAsync(int userRoleId, CancellationToken cancellationToken = default);
    Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);

    // --- בדיקת הרשאות ---
    Task<bool> HasPermissionAsync(int userId, int orgUnitId, PermissionType permission, CancellationToken cancellationToken = default);
    Task<bool> HasCrossUnitAccessAsync(int userId, CancellationToken cancellationToken = default);

    // --- יומן ביקורת ---
    Task LogActionAsync(CreateAuditLogCommand command, CancellationToken cancellationToken = default);
    Task<IEnumerable<AuditLogEntryDto>> GetAuditLogsAsync(AuditLogQueryParams query, CancellationToken cancellationToken = default);
}
