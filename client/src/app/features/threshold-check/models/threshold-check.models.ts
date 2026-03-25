export enum ConditionType {
  Age = 'Age',
  Education = 'Education',
  Score = 'Score',
  Custom = 'Custom',
}

export const CONDITION_TYPE_LABELS: Record<string, string> = {
  [ConditionType.Age]: 'גיל',
  [ConditionType.Education]: 'השכלה',
  [ConditionType.Score]: 'ציון',
  [ConditionType.Custom]: 'מותאם אישית',
};

export interface ThresholdCondition {
  id: number;
  callForCandidatesId: number;
  fieldName: string;
  operator: string;
  value: string;
  isAutomatic: boolean;
  conditionType: ConditionType;
}

export interface ThresholdCheckResult {
  id: number;
  candidacyId: number;
  thresholdConditionId: number;
  fieldName: string;
  conditionType: ConditionType;
  passed: boolean;
  actualValue?: string;
  notes?: string;
  isAutomatic: boolean;
  checkedByUserId?: number;
  checkedAt: string;
}

export interface ThresholdCheckSummary {
  candidacyId: number;
  allPassed: boolean;
  results: ThresholdCheckResult[];
}

export interface ManualCheckRequest {
  candidacyId: number;
  conditionId: number;
  passed: boolean;
  notes?: string;
  userId: number;
}
