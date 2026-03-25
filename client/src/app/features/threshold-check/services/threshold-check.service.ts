import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  ThresholdCheckResult,
  ThresholdCheckSummary,
  ThresholdCondition,
  ManualCheckRequest,
} from '../models/threshold-check.models';

@Injectable({ providedIn: 'root' })
export class ThresholdCheckService {
  constructor(private api: ApiService) {}

  private basePath(candidacyId: number): string {
    return `candidacies/${candidacyId}/threshold-checks`;
  }

  /** שליפת כל תוצאות בדיקת הסף למועמדות */
  getResults(candidacyId: number): Observable<ThresholdCheckResult[]> {
    return this.api.get<ThresholdCheckResult[]>(this.basePath(candidacyId));
  }

  /** הרצת בדיקה אוטומטית לכל תנאי הסף */
  evaluateAll(candidacyId: number): Observable<ThresholdCheckSummary> {
    return this.api.post<ThresholdCheckSummary>(
      `${this.basePath(candidacyId)}/check-all`,
      {}
    );
  }

  /** בדיקה ידנית לתנאי סף */
  submitManualCheck(
    candidacyId: number,
    request: ManualCheckRequest
  ): Observable<ThresholdCheckResult> {
    return this.api.post<ThresholdCheckResult>(
      `${this.basePath(candidacyId)}/manual`,
      request
    );
  }
}
