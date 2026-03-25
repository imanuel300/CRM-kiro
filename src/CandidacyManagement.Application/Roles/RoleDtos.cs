using CandidacyManagement.Domain.Enums;

namespace CandidacyManagement.Application.Roles;

// --- DTOs ---

public record RoleDto(
    int Id,
    string Name,
    string? Description,
    int OrgUnitId,
    bool AllowCrossUnit,
    List<PermissionType> Permissions);

public record UserRoleDto(
    int Id,
    int UserId,
    int RoleId,
    int OrgUnitId,
    string RoleName,
    DateTime AssignedAt);

public record AuditLogEntryDto(
    int Id,
    int UserId,
    string Action,
    string EntityType,
    int? EntityId,
    int? OrgUnitId,
    DateTime Timestamp,
    string? Details);

// --- Commands ---

public record CreateRoleCommand(
    string Name,
    string? Description,
    int OrgUnitId,
    bool AllowCrossUnit,
    List<PermissionType> Permissions);

public record UpdateRoleCommand(
    int Id,
    string Name,
    string? Description,
    bool AllowCrossUnit,
    List<PermissionType> Permissions);

public record AssignUserRoleCommand(
    int UserId,
    int RoleId,
    int OrgUnitId);

public record CreateAuditLogCommand(
    int UserId,
    string Action,
    string EntityType,
    int? EntityId,
    int? OrgUnitId,
    string? Details);

// --- Query Params ---

public record AuditLogQueryParams(
    int? UserId = null,
    int? OrgUnitId = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null);
