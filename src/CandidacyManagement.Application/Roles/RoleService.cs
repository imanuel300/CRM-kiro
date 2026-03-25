using CandidacyManagement.Domain.Common;
using CandidacyManagement.Domain.Entities;
using CandidacyManagement.Domain.Enums;
using CandidacyManagement.Domain.Exceptions;

namespace CandidacyManagement.Application.Roles;

/// <summary>
/// שירות ניהול תפקידים, הרשאות, שיוך משתמשים ויומן ביקורת
/// </summary>
public class RoleService : IRoleService
{
    private readonly IRepository<Role> _roleRepository;
    private readonly IRepository<Permission> _permissionRepository;
    private readonly IRepository<UserRole> _userRoleRepository;
    private readonly IRepository<AuditLogEntry> _auditLogRepository;
    private readonly IRepository<OrganizationalUnit> _orgUnitRepository;

    public RoleService(
        IRepository<Role> roleRepository,
        IRepository<Permission> permissionRepository,
        IRepository<UserRole> userRoleRepository,
        IRepository<AuditLogEntry> auditLogRepository,
        IRepository<OrganizationalUnit> orgUnitRepository)
    {
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _userRoleRepository = userRoleRepository;
        _auditLogRepository = auditLogRepository;
        _orgUnitRepository = orgUnitRepository;
    }

    // --- ניהול תפקידים ---

    public async Task<RoleDto> CreateRoleAsync(CreateRoleCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "שם התפקיד הוא שדה חובה");

        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        // בדיקת כפילות שם תפקיד באותה יחידה
        var existingRoles = await _roleRepository.FindAsync(
            r => r.OrgUnitId == command.OrgUnitId && r.Name == command.Name, cancellationToken);
        if (existingRoles.Any())
            throw new ValidationException("Name", "תפקיד עם שם זהה כבר קיים ביחידה הארגונית");

        var role = new Role
        {
            Name = command.Name,
            Description = command.Description,
            OrgUnitId = command.OrgUnitId,
            AllowCrossUnit = command.AllowCrossUnit
        };

        await _roleRepository.AddAsync(role, cancellationToken);

        // הוספת הרשאות
        foreach (var permType in command.Permissions.Distinct())
        {
            var permission = new Permission
            {
                RoleId = role.Id,
                PermissionType = permType
            };
            await _permissionRepository.AddAsync(permission, cancellationToken);
            role.Permissions.Add(permission);
        }

        return ToRoleDto(role);
    }

    public async Task<RoleDto> UpdateRoleAsync(UpdateRoleCommand command, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
            throw new ValidationException("Name", "שם התפקיד הוא שדה חובה");

        var role = await _roleRepository.GetByIdAsync(command.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), command.Id);

        // בדיקת כפילות שם (לא כולל את עצמו)
        var existingRoles = await _roleRepository.FindAsync(
            r => r.OrgUnitId == role.OrgUnitId && r.Name == command.Name && r.Id != command.Id, cancellationToken);
        if (existingRoles.Any())
            throw new ValidationException("Name", "תפקיד עם שם זהה כבר קיים ביחידה הארגונית");

        role.Name = command.Name;
        role.Description = command.Description;
        role.AllowCrossUnit = command.AllowCrossUnit;
        role.UpdatedAt = DateTime.UtcNow;

        await _roleRepository.UpdateAsync(role, cancellationToken);

        // עדכון הרשאות - מחיקת קיימות והוספת חדשות
        var existingPermissions = await _permissionRepository.FindAsync(
            p => p.RoleId == role.Id, cancellationToken);
        foreach (var perm in existingPermissions)
            await _permissionRepository.DeleteAsync(perm, cancellationToken);

        role.Permissions.Clear();
        foreach (var permType in command.Permissions.Distinct())
        {
            var permission = new Permission
            {
                RoleId = role.Id,
                PermissionType = permType
            };
            await _permissionRepository.AddAsync(permission, cancellationToken);
            role.Permissions.Add(permission);
        }

        return ToRoleDto(role);
    }

    public async Task<RoleDto> GetRoleByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), id);

        var permissions = await _permissionRepository.FindAsync(
            p => p.RoleId == role.Id, cancellationToken);
        role.Permissions = permissions.ToList();

        return ToRoleDto(role);
    }

    public async Task<IEnumerable<RoleDto>> GetRolesByOrgUnitAsync(int orgUnitId, CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.FindAsync(
            r => r.OrgUnitId == orgUnitId, cancellationToken);

        var result = new List<RoleDto>();
        foreach (var role in roles)
        {
            var permissions = await _permissionRepository.FindAsync(
                p => p.RoleId == role.Id, cancellationToken);
            role.Permissions = permissions.ToList();
            result.Add(ToRoleDto(role));
        }

        return result;
    }

    public async Task DeleteRoleAsync(int id, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), id);

        // מחיקת הרשאות קשורות
        var permissions = await _permissionRepository.FindAsync(
            p => p.RoleId == id, cancellationToken);
        foreach (var perm in permissions)
            await _permissionRepository.DeleteAsync(perm, cancellationToken);

        // מחיקת שיוכי משתמשים
        var userRoles = await _userRoleRepository.FindAsync(
            ur => ur.RoleId == id, cancellationToken);
        foreach (var ur in userRoles)
            await _userRoleRepository.DeleteAsync(ur, cancellationToken);

        await _roleRepository.DeleteAsync(role, cancellationToken);
    }

    // --- שיוך משתמשים לתפקידים ---

    public async Task<UserRoleDto> AssignUserRoleAsync(AssignUserRoleCommand command, CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(command.RoleId, cancellationToken)
            ?? throw new NotFoundException(nameof(Role), command.RoleId);

        _ = await _orgUnitRepository.GetByIdAsync(command.OrgUnitId, cancellationToken)
            ?? throw new NotFoundException(nameof(OrganizationalUnit), command.OrgUnitId);

        // בדיקה שהמשתמש לא משויך כבר לאותו תפקיד באותה יחידה
        var existing = await _userRoleRepository.FindAsync(
            ur => ur.UserId == command.UserId && ur.RoleId == command.RoleId && ur.OrgUnitId == command.OrgUnitId,
            cancellationToken);
        if (existing.Any())
            throw new ValidationException("UserRole", "המשתמש כבר משויך לתפקיד זה ביחידה הארגונית");

        var userRole = new UserRole
        {
            UserId = command.UserId,
            RoleId = command.RoleId,
            OrgUnitId = command.OrgUnitId,
            AssignedAt = DateTime.UtcNow
        };

        await _userRoleRepository.AddAsync(userRole, cancellationToken);

        return new UserRoleDto(
            userRole.Id, userRole.UserId, userRole.RoleId,
            userRole.OrgUnitId, role.Name, userRole.AssignedAt);
    }

    public async Task RemoveUserRoleAsync(int userRoleId, CancellationToken cancellationToken = default)
    {
        var userRole = await _userRoleRepository.GetByIdAsync(userRoleId, cancellationToken)
            ?? throw new NotFoundException(nameof(UserRole), userRoleId);

        await _userRoleRepository.DeleteAsync(userRole, cancellationToken);
    }

    public async Task<IEnumerable<UserRoleDto>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.FindAsync(
            ur => ur.UserId == userId, cancellationToken);

        var result = new List<UserRoleDto>();
        foreach (var ur in userRoles)
        {
            var role = await _roleRepository.GetByIdAsync(ur.RoleId, cancellationToken);
            result.Add(new UserRoleDto(
                ur.Id, ur.UserId, ur.RoleId,
                ur.OrgUnitId, role?.Name ?? "", ur.AssignedAt));
        }

        return result;
    }

    // --- בדיקת הרשאות ---

    public async Task<bool> HasPermissionAsync(int userId, int orgUnitId, PermissionType permission, CancellationToken cancellationToken = default)
    {
        // שליפת כל שיוכי התפקידים של המשתמש
        var userRoles = await _userRoleRepository.FindAsync(
            ur => ur.UserId == userId, cancellationToken);

        foreach (var ur in userRoles)
        {
            // בדיקה ישירה - המשתמש משויך ליחידה הנדרשת
            bool matchesOrgUnit = ur.OrgUnitId == orgUnitId;

            // בדיקת גישה חוצת-יחידות
            if (!matchesOrgUnit)
            {
                var role = await _roleRepository.GetByIdAsync(ur.RoleId, cancellationToken);
                if (role == null || !role.AllowCrossUnit)
                    continue;
            }

            // בדיקת ההרשאה הספציפית
            var hasPermission = await _permissionRepository.ExistsAsync(
                p => p.RoleId == ur.RoleId && p.PermissionType == permission, cancellationToken);
            if (hasPermission)
                return true;
        }

        return false;
    }

    public async Task<bool> HasCrossUnitAccessAsync(int userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.FindAsync(
            ur => ur.UserId == userId, cancellationToken);

        foreach (var ur in userRoles)
        {
            var role = await _roleRepository.GetByIdAsync(ur.RoleId, cancellationToken);
            if (role?.AllowCrossUnit == true)
                return true;
        }

        return false;
    }

    // --- יומן ביקורת ---

    public async Task LogActionAsync(CreateAuditLogCommand command, CancellationToken cancellationToken = default)
    {
        var entry = new AuditLogEntry
        {
            UserId = command.UserId,
            Action = command.Action,
            EntityType = command.EntityType,
            EntityId = command.EntityId,
            OrgUnitId = command.OrgUnitId,
            Details = command.Details,
            Timestamp = DateTime.UtcNow
        };

        await _auditLogRepository.AddAsync(entry, cancellationToken);
    }

    public async Task<IEnumerable<AuditLogEntryDto>> GetAuditLogsAsync(AuditLogQueryParams query, CancellationToken cancellationToken = default)
    {
        var logs = await _auditLogRepository.FindAsync(log =>
            (!query.UserId.HasValue || log.UserId == query.UserId.Value) &&
            (!query.OrgUnitId.HasValue || log.OrgUnitId == query.OrgUnitId.Value) &&
            (!query.FromDate.HasValue || log.Timestamp >= query.FromDate.Value) &&
            (!query.ToDate.HasValue || log.Timestamp <= query.ToDate.Value),
            cancellationToken);

        return logs.Select(ToAuditLogDto).ToList();
    }

    // --- Mapping helpers ---

    private static RoleDto ToRoleDto(Role role) =>
        new(role.Id, role.Name, role.Description, role.OrgUnitId, role.AllowCrossUnit,
            role.Permissions.Select(p => p.PermissionType).ToList());

    private static AuditLogEntryDto ToAuditLogDto(AuditLogEntry entry) =>
        new(entry.Id, entry.UserId, entry.Action, entry.EntityType,
            entry.EntityId, entry.OrgUnitId, entry.Timestamp, entry.Details);
}
