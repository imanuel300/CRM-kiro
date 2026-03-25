import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import { DashboardDataDto, OrgUnitDashboardSummaryDto } from '../models/dashboard.models';

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly basePath = 'dashboard';

  constructor(private api: ApiService) {}

  getDashboardData(orgUnitId: number): Observable<DashboardDataDto> {
    return this.api.get<DashboardDataDto>(this.basePath, { orgUnitId });
  }

  getDashboardByOrgUnit(orgUnitId: number): Observable<OrgUnitDashboardSummaryDto> {
    return this.api.get<OrgUnitDashboardSummaryDto>(`${this.basePath}/org-unit/${orgUnitId}`);
  }
}
