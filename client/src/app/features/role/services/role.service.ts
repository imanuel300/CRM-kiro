import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Role,
  UserRole,
  AuditLogEntry,
  CreateRoleCommand,
  UpdateRoleCommand,
  AssignUserRoleCommand,
  AuditLogQueryParams,
} from '../models/role.models';

@Injectable({ providedIn: 'root' })
export class RoleApiService {
  private readonly basePath = 'roles';

  constructor(private api: ApiService) {}

  // --- Roles ---

  listByOrgUnit(orgUnitId: number): Observable<Role[]> {
    return this.api.get<Role[]>(this.basePath, { orgUnitId });
  }

  getById(id: number): Observable<Role> {
    return this.api.get<Role>(`${this.basePath}/${id}`);
  }

  create(command: CreateRoleCommand): Observable<Role> {
    return this.api.post<Role>(this.basePath, command);
  }

  update(command: UpdateRoleCommand): Observable<Role> {
    return this.api.put<Role>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  // --- User Assignments ---

  assignUser(command: AssignUserRoleCommand): Observable<UserRole> {
    return this.api.post<UserRole>(`${this.basePath}/assignments`, command);
  }

  removeAssignment(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/assignments/${id}`);
  }

  getUserRoles(userId: number): Observable<UserRole[]> {
    return this.api.get<UserRole[]>(`${this.basePath}/users/${userId}/roles`);
  }

  // --- Audit Logs ---

  getAuditLogs(params?: AuditLogQueryParams): Observable<AuditLogEntry[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params?.userId) queryParams['userId'] = params.userId;
    if (params?.orgUnitId) queryParams['orgUnitId'] = params.orgUnitId;
    if (params?.fromDate) queryParams['fromDate'] = params.fromDate;
    if (params?.toDate) queryParams['toDate'] = params.toDate;
    return this.api.get<AuditLogEntry[]>(`${this.basePath}/audit-logs`, queryParams);
  }
}
