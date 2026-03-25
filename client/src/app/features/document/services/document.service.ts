import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Document,
  DocumentVersion,
  RequiredDocument,
  UploadDocumentCommand,
  ReviewDocumentCommand,
  DocumentQueryParams,
  DocumentCompletenessResult,
} from '../models/document.models';

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private readonly basePath = 'documents';

  constructor(private api: ApiService) {}

  list(params?: DocumentQueryParams): Observable<Document[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params?.candidacyId) queryParams['candidacyId'] = params.candidacyId;
    if (params?.documentType) queryParams['documentType'] = params.documentType;
    if (params?.status) queryParams['status'] = params.status;
    return this.api.get<Document[]>(this.basePath, queryParams);
  }

  getById(id: number): Observable<Document> {
    return this.api.get<Document>(`${this.basePath}/${id}`);
  }

  upload(command: UploadDocumentCommand): Observable<Document> {
    return this.api.post<Document>(this.basePath, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  review(id: number, command: ReviewDocumentCommand): Observable<Document> {
    return this.api.post<Document>(`${this.basePath}/${id}/review`, command);
  }

  getVersionHistory(candidacyId: number, documentType: string): Observable<DocumentVersion[]> {
    return this.api.get<DocumentVersion[]>(
      `${this.basePath}/candidacies/${candidacyId}/types/${documentType}/versions`
    );
  }

  getRequiredDocuments(callForCandidatesId?: number, orgUnitId?: number): Observable<RequiredDocument[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (callForCandidatesId) queryParams['callForCandidatesId'] = callForCandidatesId;
    if (orgUnitId) queryParams['orgUnitId'] = orgUnitId;
    return this.api.get<RequiredDocument[]>(`${this.basePath}/required`, queryParams);
  }

  checkCompleteness(candidacyId: number): Observable<DocumentCompletenessResult> {
    return this.api.get<DocumentCompletenessResult>(
      `${this.basePath}/candidacies/${candidacyId}/completeness`
    );
  }
}
