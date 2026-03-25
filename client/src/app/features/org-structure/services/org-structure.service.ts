import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  OrgSubUnit,
  OrgSubUnitTree,
  CreateSubUnitCommand,
  UpdateSubUnitCommand,
  OrgPosition,
  CreatePositionCommand,
  UpdatePositionCommand,
  AssignToPositionCommand,
  UnassignFromPositionCommand,
  PositionAssignment,
  SubUnitOccupancy,
} from '../models/org-structure.models';

@Injectable({ providedIn: 'root' })
export class OrgStructureApiService {
  private readonly basePath = 'org-structure';

  constructor(private api: ApiService) {}

  // Sub-Unit operations
  createSubUnit(command: CreateSubUnitCommand): Observable<OrgSubUnit> {
    return this.api.post<OrgSubUnit>(`${this.basePath}/sub-units`, command);
  }

  updateSubUnit(command: UpdateSubUnitCommand): Observable<OrgSubUnit> {
    return this.api.put<OrgSubUnit>(`${this.basePath}/sub-units/${command.id}`, command);
  }

  deleteSubUnit(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/sub-units/${id}`);
  }

  getTree(orgUnitId: number): Observable<OrgSubUnitTree> {
    return this.api.get<OrgSubUnitTree>(`${this.basePath}/tree/${orgUnitId}`);
  }

  // Position operations
  createPosition(command: CreatePositionCommand): Observable<OrgPosition> {
    return this.api.post<OrgPosition>(`${this.basePath}/positions`, command);
  }

  updatePosition(command: UpdatePositionCommand): Observable<OrgPosition> {
    return this.api.put<OrgPosition>(`${this.basePath}/positions/${command.id}`, command);
  }

  deletePosition(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/positions/${id}`);
  }

  // Assignment operations
  assignToPosition(command: AssignToPositionCommand): Observable<PositionAssignment> {
    return this.api.post<PositionAssignment>(`${this.basePath}/assign`, command);
  }

  unassignFromPosition(command: UnassignFromPositionCommand): Observable<void> {
    return this.api.post<void>(`${this.basePath}/unassign`, command);
  }

  // Occupancy
  getOccupancy(subUnitId: number): Observable<SubUnitOccupancy> {
    return this.api.get<SubUnitOccupancy>(`${this.basePath}/occupancy/${subUnitId}`);
  }
}
