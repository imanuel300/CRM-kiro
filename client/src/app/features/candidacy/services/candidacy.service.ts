import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Candidacy,
  CandidacyDetail,
  CandidacyCustomFieldValue,
  CreateCandidacyCommand,
  UpdateCandidacyCommand,
  SetCandidacyCustomFieldValueCommand,
  CandidacyQueryParams,
  TransitionStatusCommand,
  StatusHistory,
} from '../models/candidacy.models';

@Injectable({ providedIn: 'root' })
export class CandidacyService {
  private readonly basePath = 'candidacies';

  constructor(private api: ApiService) {}

  list(params?: CandidacyQueryParams): Observable<Candidacy[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params?.orgUnitId) queryParams['orgUnitId'] = params.orgUnitId;
    if (params?.contactId) queryParams['contactId'] = params.contactId;
    if (params?.callForCandidatesId) queryParams['callForCandidatesId'] = params.callForCandidatesId;
    if (params?.isActive !== undefined) queryParams['isActive'] = params.isActive;
    return this.api.get<Candidacy[]>(this.basePath, queryParams);
  }

  getById(id: number): Observable<Candidacy> {
    return this.api.get<Candidacy>(`${this.basePath}/${id}`);
  }

  getDetail(id: number): Observable<CandidacyDetail> {
    return this.api.get<CandidacyDetail>(`${this.basePath}/${id}/detail`);
  }

  create(command: CreateCandidacyCommand): Observable<Candidacy> {
    return this.api.post<Candidacy>(this.basePath, command);
  }

  update(command: UpdateCandidacyCommand): Observable<Candidacy> {
    return this.api.put<Candidacy>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  transitionStatus(command: TransitionStatusCommand): Observable<Candidacy> {
    return this.api.post<Candidacy>(`${this.basePath}/${command.candidacyId}/transition`, command);
  }

  getStatusHistory(candidacyId: number): Observable<StatusHistory[]> {
    return this.api.get<StatusHistory[]>(`${this.basePath}/${candidacyId}/status-history`);
  }

  getCustomFields(candidacyId: number): Observable<CandidacyCustomFieldValue[]> {
    return this.api.get<CandidacyCustomFieldValue[]>(`${this.basePath}/${candidacyId}/custom-fields`);
  }

  setCustomFieldValue(command: SetCandidacyCustomFieldValueCommand): Observable<void> {
    return this.api.put<void>(
      `${this.basePath}/${command.candidacyId}/custom-fields`,
      command
    );
  }
}
