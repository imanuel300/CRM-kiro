import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  ConflictOfInterest,
  FamilyRelation,
  CandidacyDeclarations,
  CreateConflictCommand,
  UpdateConflictCommand,
  CreateFamilyRelationCommand,
  UpdateFamilyRelationCommand,
  ReviewConflictCommand,
} from '../models/conflict.models';

@Injectable({ providedIn: 'root' })
export class ConflictService {
  private readonly basePath = 'conflicts-of-interest';

  constructor(private api: ApiService) {}

  // --- ניגוד עניינים ---

  getConflict(id: number): Observable<ConflictOfInterest> {
    return this.api.get<ConflictOfInterest>(`${this.basePath}/${id}`);
  }

  createConflict(command: CreateConflictCommand): Observable<ConflictOfInterest> {
    return this.api.post<ConflictOfInterest>(this.basePath, command);
  }

  updateConflict(command: UpdateConflictCommand): Observable<ConflictOfInterest> {
    return this.api.put<ConflictOfInterest>(`${this.basePath}/${command.id}`, command);
  }

  deleteConflict(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  reviewConflict(command: ReviewConflictCommand): Observable<ConflictOfInterest> {
    return this.api.post<ConflictOfInterest>(`${this.basePath}/${command.id}/review`, command);
  }

  // --- קרבה משפחתית ---

  getFamilyRelation(id: number): Observable<FamilyRelation> {
    return this.api.get<FamilyRelation>(`${this.basePath}/family-relations/${id}`);
  }

  createFamilyRelation(command: CreateFamilyRelationCommand): Observable<FamilyRelation> {
    return this.api.post<FamilyRelation>(`${this.basePath}/family-relations`, command);
  }

  updateFamilyRelation(command: UpdateFamilyRelationCommand): Observable<FamilyRelation> {
    return this.api.put<FamilyRelation>(
      `${this.basePath}/family-relations/${command.id}`,
      command
    );
  }

  deleteFamilyRelation(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/family-relations/${id}`);
  }

  // --- הצהרות למועמדות ---

  getDeclarationsForCandidacy(candidacyId: number): Observable<CandidacyDeclarations> {
    return this.api.get<CandidacyDeclarations>(`${this.basePath}/candidacy/${candidacyId}`);
  }

  // --- בדיקה ידנית ---

  getCandidacyIdsRequiringManualReview(orgUnitId?: number): Observable<number[]> {
    const params: Record<string, string | number | boolean> = {};
    if (orgUnitId != null) {
      params['orgUnitId'] = orgUnitId;
    }
    return this.api.get<number[]>(`${this.basePath}/manual-review`, params);
  }
}
