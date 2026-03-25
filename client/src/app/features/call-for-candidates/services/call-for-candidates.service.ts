import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  CallForCandidates,
  CallForCandidatesDetail,
  CallForCandidatesQueryParams,
  CreateCallForCandidatesCommand,
  UpdateCallForCandidatesCommand,
  ThresholdCondition,
  CreateThresholdConditionCommand,
  Position,
  CreatePositionCommand,
  ClosingSummary,
} from '../models/call-for-candidates.models';

@Injectable({ providedIn: 'root' })
export class CallForCandidatesService {
  private readonly basePath = 'calls-for-candidates';

  constructor(private api: ApiService) {}

  list(params?: CallForCandidatesQueryParams): Observable<CallForCandidates[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params?.orgUnitId) queryParams['orgUnitId'] = params.orgUnitId;
    if (params?.isActive !== undefined) queryParams['isActive'] = params.isActive;
    if (params?.isTender !== undefined) queryParams['isTender'] = params.isTender;
    return this.api.get<CallForCandidates[]>(this.basePath, queryParams);
  }

  getById(id: number): Observable<CallForCandidates> {
    return this.api.get<CallForCandidates>(`${this.basePath}/${id}`);
  }

  getDetail(id: number): Observable<CallForCandidatesDetail> {
    return this.api.get<CallForCandidatesDetail>(`${this.basePath}/${id}/detail`);
  }

  create(command: CreateCallForCandidatesCommand): Observable<CallForCandidates> {
    return this.api.post<CallForCandidates>(this.basePath, command);
  }

  update(command: UpdateCallForCandidatesCommand): Observable<CallForCandidates> {
    return this.api.put<CallForCandidates>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  // --- Threshold Conditions ---

  getThresholdConditions(callId: number): Observable<ThresholdCondition[]> {
    return this.api.get<ThresholdCondition[]>(`${this.basePath}/${callId}/threshold-conditions`);
  }

  addThresholdCondition(command: CreateThresholdConditionCommand): Observable<ThresholdCondition> {
    return this.api.post<ThresholdCondition>(
      `${this.basePath}/${command.callForCandidatesId}/threshold-conditions`,
      command
    );
  }

  removeThresholdCondition(callId: number, conditionId: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${callId}/threshold-conditions/${conditionId}`);
  }

  // --- Positions ---

  getPositions(callId: number): Observable<Position[]> {
    return this.api.get<Position[]>(`${this.basePath}/${callId}/positions`);
  }

  addPosition(command: CreatePositionCommand): Observable<Position> {
    return this.api.post<Position>(
      `${this.basePath}/${command.callForCandidatesId}/positions`,
      command
    );
  }

  removePosition(callId: number, positionId: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${callId}/positions/${positionId}`);
  }

  // --- Closing ---

  isClosed(callId: number): Observable<boolean> {
    return this.api.get<boolean>(`${this.basePath}/${callId}/is-closed`);
  }

  getClosingSummary(callId: number): Observable<ClosingSummary> {
    return this.api.get<ClosingSummary>(`${this.basePath}/${callId}/closing-summary`);
  }
}
