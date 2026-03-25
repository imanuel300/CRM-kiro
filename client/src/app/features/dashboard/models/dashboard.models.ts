export interface DashboardDataDto {
  orgUnitId: number;
  orgUnitName: string;
  activeCandidacies: number;
  byScreeningStage: StageBreakdownDto[];
  candidaciesRequiringAttention: number;
  attentionItems: AttentionItemDto[];
}

export interface StageBreakdownDto {
  stageCategory: string;
  stageDisplayName: string;
  count: number;
}

export interface AttentionItemDto {
  candidacyId: number;
  contactId: number;
  statusCode: string;
  statusDisplayName: string;
  lastUpdated: string;
  reason: string;
}

export interface OrgUnitDashboardSummaryDto {
  orgUnitId: number;
  orgUnitName: string;
  activeCandidacies: number;
  candidaciesRequiringAttention: number;
  byScreeningStage: StageBreakdownDto[];
}
