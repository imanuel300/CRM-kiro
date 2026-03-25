import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Interview,
  InterviewFeedback,
  CreateInterviewCommand,
  UpdateInterviewCommand,
  SubmitFeedbackCommand,
  InterviewQueryParams,
} from '../models/interview.models';

@Injectable({ providedIn: 'root' })
export class InterviewService {
  private readonly basePath = 'interviews';

  constructor(private api: ApiService) {}

  list(params?: InterviewQueryParams): Observable<Interview[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params?.orgUnitId) queryParams['orgUnitId'] = params.orgUnitId;
    if (params?.callForCandidatesId) queryParams['callForCandidatesId'] = params.callForCandidatesId;
    if (params?.candidacyId) queryParams['candidacyId'] = params.candidacyId;
    return this.api.get<Interview[]>(this.basePath, queryParams);
  }

  getById(id: number): Observable<Interview> {
    return this.api.get<Interview>(`${this.basePath}/${id}`);
  }

  create(command: CreateInterviewCommand): Observable<Interview> {
    return this.api.post<Interview>(this.basePath, command);
  }

  update(command: UpdateInterviewCommand): Observable<Interview> {
    return this.api.put<Interview>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  submitFeedback(command: SubmitFeedbackCommand): Observable<InterviewFeedback> {
    return this.api.post<InterviewFeedback>(
      `${this.basePath}/${command.interviewId}/feedback`,
      command
    );
  }

  getFeedback(interviewId: number): Observable<InterviewFeedback[]> {
    return this.api.get<InterviewFeedback[]>(`${this.basePath}/${interviewId}/feedback`);
  }

  scheduleSecondInterview(interviewId: number, command: CreateInterviewCommand): Observable<Interview> {
    return this.api.post<Interview>(
      `${this.basePath}/${interviewId}/second-interview`,
      command
    );
  }
}
