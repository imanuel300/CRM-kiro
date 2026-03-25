import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Exam,
  ExamScore,
  CreateExamCommand,
  UpdateExamCommand,
  SubmitScoreCommand,
  SubmitAppealCommand,
  ExamQueryParams,
} from '../models/exam.models';

@Injectable({ providedIn: 'root' })
export class ExamService {
  private readonly basePath = 'exams';

  constructor(private api: ApiService) {}

  list(params?: ExamQueryParams): Observable<Exam[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params?.orgUnitId) queryParams['orgUnitId'] = params.orgUnitId;
    if (params?.callForCandidatesId) queryParams['callForCandidatesId'] = params.callForCandidatesId;
    return this.api.get<Exam[]>(this.basePath, queryParams);
  }

  getById(id: number): Observable<Exam> {
    return this.api.get<Exam>(`${this.basePath}/${id}`);
  }

  create(command: CreateExamCommand): Observable<Exam> {
    return this.api.post<Exam>(this.basePath, command);
  }

  update(command: UpdateExamCommand): Observable<Exam> {
    return this.api.put<Exam>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  submitScore(command: SubmitScoreCommand): Observable<ExamScore> {
    return this.api.post<ExamScore>(`${this.basePath}/${command.examId}/scores`, command);
  }

  getScoresByExam(examId: number): Observable<ExamScore[]> {
    return this.api.get<ExamScore[]>(`${this.basePath}/${examId}/scores`);
  }

  getScore(examId: number, candidacyId: number): Observable<ExamScore> {
    return this.api.get<ExamScore>(`${this.basePath}/${examId}/scores/${candidacyId}`);
  }

  submitAppeal(command: SubmitAppealCommand): Observable<ExamScore> {
    return this.api.post<ExamScore>(`${this.basePath}/${command.examId}/appeals`, command);
  }
}
