export interface CallForCandidates {
  id: number;
  orgUnitId: number;
  title: string;
  description?: string;
  openDate: string;
  closeDate?: string;
  isTender: boolean;
  minScore?: number;
  eligibilityConditions?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CallForCandidatesDetail extends CallForCandidates {
  thresholdConditions: ThresholdCondition[];
  positions: Position[];
}

export interface ThresholdCondition {
  id: number;
  callForCandidatesId: number;
  fieldName: string;
  operator: string;
  value: string;
  isAutomatic: boolean;
}

export interface Position {
  id: number;
  callForCandidatesId: number;
  title: string;
  description?: string;
  vacancies: number;
}

export interface CreateCallForCandidatesCommand {
  orgUnitId: number;
  title: string;
  description?: string;
  openDate: string;
  closeDate?: string;
  isTender: boolean;
  minScore?: number;
  eligibilityConditions?: string;
}

export interface UpdateCallForCandidatesCommand {
  id: number;
  title: string;
  description?: string;
  openDate: string;
  closeDate?: string;
  isTender: boolean;
  minScore?: number;
  eligibilityConditions?: string;
}

export interface CreateThresholdConditionCommand {
  callForCandidatesId: number;
  fieldName: string;
  operator: string;
  value: string;
  isAutomatic: boolean;
}

export interface CreatePositionCommand {
  callForCandidatesId: number;
  title: string;
  description?: string;
  vacancies: number;
}

export interface CallForCandidatesQueryParams {
  orgUnitId?: number;
  isActive?: boolean;
  isTender?: boolean;
}

export interface ClosingSummary {
  callForCandidatesId: number;
  title: string;
  closeDate?: string;
  totalCandidacies: number;
  metThreshold: number;
  rejected: number;
}
