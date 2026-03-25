import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  OrgUnit,
  CreateOrgUnitCommand,
  UpdateOrgUnitCommand,
  WorkflowDefinition,
  ConfigureWorkflowCommand,
  StatusDefinition,
  ConfigureStatusesCommand,
  StatusTransition,
  ConfigureTransitionsCommand,
} from '../models/org-unit.models';

@Injectable({ providedIn: 'root' })
export class OrgUnitService {
  private readonly basePath = 'org-units';

  constructor(private api: ApiService) {}

  getAll(): Observable<OrgUnit[]> {
    return this.api.get<OrgUnit[]>(this.basePath);
  }

  getById(id: number): Observable<OrgUnit> {
    return this.api.get<OrgUnit>(`${this.basePath}/${id}`);
  }

  create(command: CreateOrgUnitCommand): Observable<OrgUnit> {
    return this.api.post<OrgUnit>(this.basePath, command);
  }

  update(command: UpdateOrgUnitCommand): Observable<OrgUnit> {
    return this.api.put<OrgUnit>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  getWorkflow(orgUnitId: number): Observable<WorkflowDefinition> {
    return this.api.get<WorkflowDefinition>(`${this.basePath}/${orgUnitId}/workflow`);
  }

  configureWorkflow(command: ConfigureWorkflowCommand): Observable<WorkflowDefinition> {
    return this.api.put<WorkflowDefinition>(
      `${this.basePath}/${command.orgUnitId}/workflow`,
      command
    );
  }

  getStatuses(orgUnitId: number): Observable<StatusDefinition[]> {
    return this.api.get<StatusDefinition[]>(`${this.basePath}/${orgUnitId}/statuses`);
  }

  configureStatuses(command: ConfigureStatusesCommand): Observable<StatusDefinition[]> {
    return this.api.put<StatusDefinition[]>(
      `${this.basePath}/${command.orgUnitId}/statuses`,
      command
    );
  }

  getTransitions(orgUnitId: number): Observable<StatusTransition[]> {
    return this.api.get<StatusTransition[]>(`${this.basePath}/${orgUnitId}/transitions`);
  }

  configureTransitions(command: ConfigureTransitionsCommand): Observable<StatusTransition[]> {
    return this.api.put<StatusTransition[]>(
      `${this.basePath}/${command.orgUnitId}/transitions`,
      command
    );
  }
}
