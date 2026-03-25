export enum PermissionType {
  View = 'View',
  Edit = 'Edit',
  Create = 'Create',
  Delete = 'Delete',
  ChangeStatus = 'ChangeStatus',
  SendNotification = 'SendNotification',
}

export const PERMISSION_LABELS: { value: PermissionType; label: string }[] = [
  { value: PermissionType.View, label: 'צפייה' },
  { value: PermissionType.Edit, label: 'עריכה' },
  { value: PermissionType.Create, label: 'יצירה' },
  { value: PermissionType.Delete, label: 'מחיקה' },
  { value: PermissionType.ChangeStatus, label: 'שינוי סטטוס' },
  { value: PermissionType.SendNotification, label: 'שליחת דיוור' },
];

export interface Role {
  id: number;
  name: string;
  description?: string;
  orgUnitId: number;
  allowCrossUnit: boolean;
  permissions: PermissionType[];
}

export interface UserRole {
  id: number;
  userId: number;
  roleId: number;
  orgUnitId: number;
  roleName: string;
  assignedAt: string;
}

export interface AuditLogEntry {
  id: number;
  userId: number;
  action: string;
  entityType: string;
  entityId?: number;
  orgUnitId?: number;
  timestamp: string;
  details?: string;
}

export interface CreateRoleCommand {
  name: string;
  description?: string;
  orgUnitId: number;
  allowCrossUnit: boolean;
  permissions: PermissionType[];
}

export interface UpdateRoleCommand {
  id: number;
  name: string;
  description?: string;
  allowCrossUnit: boolean;
  permissions: PermissionType[];
}

export interface AssignUserRoleCommand {
  userId: number;
  roleId: number;
  orgUnitId: number;
}

export interface AuditLogQueryParams {
  userId?: number;
  orgUnitId?: number;
  fromDate?: string;
  toDate?: string;
}
