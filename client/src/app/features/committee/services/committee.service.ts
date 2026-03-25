import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Committee,
  CommitteeMeeting,
  CommitteeDecision,
  CommitteeAppeal,
  CreateCommitteeCommand,
  UpdateCommitteeCommand,
  CreateMeetingCommand,
  RecordDecisionCommand,
  SubmitCommitteeAppealCommand,
  CommitteeQueryParams,
} from '../models/committee.models';

@Injectable({ providedIn: 'root' })
export class CommitteeService {
  private readonly basePath = 'committees';

  constructor(private api: ApiService) {}

  list(params?: CommitteeQueryParams): Observable<Committee[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params?.orgUnitId) queryParams['orgUnitId'] = params.orgUnitId;
    return this.api.get<Committee[]>(this.basePath, queryParams);
  }

  getById(id: number): Observable<Committee> {
    return this.api.get<Committee>(`${this.basePath}/${id}`);
  }

  create(command: CreateCommitteeCommand): Observable<Committee> {
    return this.api.post<Committee>(this.basePath, command);
  }

  update(command: UpdateCommitteeCommand): Observable<Committee> {
    return this.api.put<Committee>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  // --- Meetings ---

  listMeetings(committeeId: number): Observable<CommitteeMeeting[]> {
    return this.api.get<CommitteeMeeting[]>(`${this.basePath}/${committeeId}/meetings`);
  }

  getMeeting(committeeId: number, meetingId: number): Observable<CommitteeMeeting> {
    return this.api.get<CommitteeMeeting>(`${this.basePath}/${committeeId}/meetings/${meetingId}`);
  }

  createMeeting(committeeId: number, command: CreateMeetingCommand): Observable<CommitteeMeeting> {
    return this.api.post<CommitteeMeeting>(`${this.basePath}/${committeeId}/meetings`, command);
  }

  // --- Decisions ---

  getDecisions(committeeId: number, meetingId: number): Observable<CommitteeDecision[]> {
    return this.api.get<CommitteeDecision[]>(
      `${this.basePath}/${committeeId}/meetings/${meetingId}/decisions`
    );
  }

  recordDecision(committeeId: number, meetingId: number, command: RecordDecisionCommand): Observable<CommitteeDecision> {
    return this.api.post<CommitteeDecision>(
      `${this.basePath}/${committeeId}/meetings/${meetingId}/decisions`,
      command
    );
  }

  // --- Appeals ---

  submitAppeal(committeeId: number, meetingId: number, command: SubmitCommitteeAppealCommand): Observable<CommitteeAppeal> {
    return this.api.post<CommitteeAppeal>(
      `${this.basePath}/${committeeId}/meetings/${meetingId}/appeals`,
      command
    );
  }

  resolveAppeal(
    committeeId: number, meetingId: number, appealId: number, result: string
  ): Observable<CommitteeAppeal> {
    return this.api.put<CommitteeAppeal>(
      `${this.basePath}/${committeeId}/meetings/${meetingId}/appeals/${appealId}`,
      { appealId, result }
    );
  }

  // --- Protocol ---

  getProtocol(committeeId: number, meetingId: number): Observable<string> {
    return this.api.get<string>(`${this.basePath}/${committeeId}/meetings/${meetingId}/protocol`);
  }
}
