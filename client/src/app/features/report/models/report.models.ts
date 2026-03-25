export interface StatusReportParams {
  orgUnitId: number;
  callForCandidatesId?: number;
  statusCode?: string;
  fromDate?: string;
  toDate?: string;
}

export interface CrossUnitReportParams {
  orgUnitIds?: number[];
  fromDate?: string;
  toDate?: string;
}

export interface ExportReportParams {
  reportType: string;
  orgUnitId?: number;
  callForCandidatesId?: number;
  statusCode?: string;
  fromDate?: string;
  toDate?: string;
  orgUnitIds?: number[];
}

export interface CustomReportParams {
  orgUnitId: number;
  customReportDefinitionId: number;
  fromDate?: string;
  toDate?: string;
}

export interface StatusReportDto {
  orgUnitId: number;
  orgUnitName: string;
  totalCandidacies: number;
  byStatus: StatusBreakdownDto[];
  byCall: CallBreakdownDto[];
}

export interface StatusBreakdownDto {
  statusCode: string;
  statusDisplayName: string;
  count: number;
}

export interface CallBreakdownDto {
  callForCandidatesId: number;
  callTitle: string;
  totalCandidacies: number;
  byStatus: StatusBreakdownDto[];
}

export interface CrossUnitReportDto {
  generatedAt: string;
  totalCandidacies: number;
  units: UnitSummaryDto[];
}

export interface UnitSummaryDto {
  orgUnitId: number;
  orgUnitName: string;
  totalCandidacies: number;
  activeCandidacies: number;
  byStatus: StatusBreakdownDto[];
}

export interface ReportResult {
  reportName: string;
  generatedAt: string;
  totalRecords: number;
  rows: ReportRow[];
  aggregations: ReportAggregation[];
}

export interface ReportRow {
  values: Record<string, unknown>;
}

export interface ReportAggregation {
  groupKey: string;
  groupValue: string;
  count: number;
}

export interface CustomReportDefinitionDto {
  id: number;
  orgUnitId: number;
  name: string;
  description?: string;
  columnsJson: string;
  filtersJson: string;
  groupByJson?: string;
  sortOrderJson?: string;
  isActive: boolean;
}

export type ReportType = 'status' | 'cross-unit' | 'custom';

export const REPORT_TYPES: { value: ReportType; label: string }[] = [
  { value: 'status', label: 'דוח סטטוס מועמדויות' },
  { value: 'cross-unit', label: 'דוח מאוחד חוצה יחידות' },
  { value: 'custom', label: 'דוח מותאם אישית' },
];
