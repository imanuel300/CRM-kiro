import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Quota,
  CreateQuotaCommand,
  UpdateQuotaCommand,
  AssignCandidacyCommand,
  UnassignCandidacyCommand,
  OrgUnitFulfillment,
} from '../models/quota.models';

@Injectable({ providedIn: 'root' })
export class QuotaApiService {
  private readonly basePath = 'quotas';

  constructor(private api: ApiService) {}

  getById(id: number): Observable<Quota> {
    return this.api.get<Quota>(`${this.basePath}/${id}`);
  }

  getByOrgUnit(orgUnitId: number): Observable<Quota[]> {
    return this.api.get<Quota[]>(`${this.basePath}/by-org-unit/${orgUnitId}`);
  }

  create(command: CreateQuotaCommand): Observable<Quota> {
    return this.api.post<Quota>(this.basePath, command);
  }

  update(command: UpdateQuotaCommand): Observable<Quota> {
    return this.api.put<Quota>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  assignCandidacy(command: AssignCandidacyCommand): Observable<void> {
    return this.api.post<void>(`${this.basePath}/${command.quotaId}/assign`, command);
  }

  unassignCandidacy(command: UnassignCandidacyCommand): Observable<void> {
    return this.api.post<void>(`${this.basePath}/${command.quotaId}/unassign`, command);
  }

  getFulfillmentStatus(orgUnitId: number): Observable<OrgUnitFulfillment> {
    return this.api.get<OrgUnitFulfillment>(`${this.basePath}/fulfillment/${orgUnitId}`);
  }
}
