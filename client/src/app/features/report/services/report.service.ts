import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  StatusReportDto,
  StatusReportParams,
  CrossUnitReportDto,
  CrossUnitReportParams,
  ExportReportParams,
  CustomReportParams,
  ReportResult,
  CustomReportDefinitionDto,
} from '../models/report.models';

@Injectable({ providedIn: 'root' })
export class ReportService {
  private readonly basePath = 'reports';

  constructor(private api: ApiService) {}

  getStatusReport(params: StatusReportParams): Observable<StatusReportDto> {
    const query: Record<string, string | number | boolean> = {
      orgUnitId: params.orgUnitId,
    };
    if (params.callForCandidatesId) query['callForCandidatesId'] = params.callForCandidatesId;
    if (params.statusCode) query['statusCode'] = params.statusCode;
    if (params.fromDate) query['fromDate'] = params.fromDate;
    if (params.toDate) query['toDate'] = params.toDate;
    return this.api.get<StatusReportDto>(`${this.basePath}/status`, query);
  }

  getCrossUnitReport(params: CrossUnitReportParams): Observable<CrossUnitReportDto> {
    const query: Record<string, string | number | boolean> = {};
    if (params.fromDate) query['fromDate'] = params.fromDate;
    if (params.toDate) query['toDate'] = params.toDate;
    return this.api.get<CrossUnitReportDto>(`${this.basePath}/cross-unit`, query);
  }

  exportToExcel(params: ExportReportParams): Observable<Blob> {
    const query: Record<string, string | number | boolean> = {
      reportType: params.reportType,
    };
    if (params.orgUnitId) query['orgUnitId'] = params.orgUnitId;
    if (params.callForCandidatesId) query['callForCandidatesId'] = params.callForCandidatesId;
    if (params.statusCode) query['statusCode'] = params.statusCode;
    if (params.fromDate) query['fromDate'] = params.fromDate;
    if (params.toDate) query['toDate'] = params.toDate;
    return this.api.get<Blob>(`${this.basePath}/export`, query);
  }

  getCustomReport(params: CustomReportParams): Observable<ReportResult> {
    const query: Record<string, string | number | boolean> = {
      orgUnitId: params.orgUnitId,
      customReportDefinitionId: params.customReportDefinitionId,
    };
    if (params.fromDate) query['fromDate'] = params.fromDate;
    if (params.toDate) query['toDate'] = params.toDate;
    return this.api.get<ReportResult>(`${this.basePath}/custom`, query);
  }

  getCustomReportDefinitions(orgUnitId: number): Observable<CustomReportDefinitionDto[]> {
    return this.api.get<CustomReportDefinitionDto[]>(`${this.basePath}/definitions`, { orgUnitId });
  }
}
