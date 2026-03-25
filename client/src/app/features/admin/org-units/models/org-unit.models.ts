export interface OrgUnit {
  id: number;
  name: string;
  description?: string;
  contactEmail?: string;
  contactPhone?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrgUnitCommand {
  name: string;
  description?: string;
  contactEmail?: string;
  contactPhone?: string;
}

export interface UpdateOrgUnitCommand {
  id: number;
  name: string;
  description?: string;
  contactEmail?: string;
  contactPhone?: string;
}

export enum CandidacyStatusCategory {
  Submitted = 'Submitted',
  InReview = 'InReview',
  Exam = 'Exam',
  Interview = 'Interview',
  Committee = 'Committee',
  Accepted = 'Accepted',
  Rejected = 'Rejected',
  Withdrawn = 'Withdrawn',
}

export interface SubStatusDefinition {
  id: number;
  code: string;
  displayName: string;
}

export interface StatusDefinition {
  id: number;
  orgUnitId: number;
  code: string;
  displayName: string;
  category: CandidacyStatusCategory;
  isFinal: boolean;
  isInitial: boolean;
  sortOrder: number;
  subStatuses: SubStatusDefinition[];
}

export interface StatusTransition {
  id: number;
  fromStatusId: number;
  toStatusId: number;
  fromStatusCode: string;
  toStatusCode: string;
  requiredPermission?: string;
  requiresReason: boolean;
  autoTriggerRule?: string;
}

export interface WorkflowDefinition {
  id: number;
  orgUnitId: number;
  name: string;
  examStepEnabled: boolean;
  interviewStepEnabled: boolean;
  committeeStepEnabled: boolean;
  thresholdCheckEnabled: boolean;
  stepOrder?: string;
  version: number;
  isActive: boolean;
  createdAt: string;
}

export interface ConfigureWorkflowCommand {
  orgUnitId: number;
  name: string;
  examStepEnabled: boolean;
  interviewStepEnabled: boolean;
  committeeStepEnabled: boolean;
  thresholdCheckEnabled: boolean;
  stepOrder?: string;
}

export interface ConfigureSubStatusDefinition {
  code: string;
  displayName: string;
}

export interface ConfigureStatusDefinition {
  code: string;
  displayName: string;
  category: CandidacyStatusCategory;
  isFinal: boolean;
  isInitial: boolean;
  sortOrder: number;
  subStatuses?: ConfigureSubStatusDefinition[];
}

export interface ConfigureStatusesCommand {
  orgUnitId: number;
  statuses: ConfigureStatusDefinition[];
}

export interface ConfigureTransitionDefinition {
  fromStatusCode: string;
  toStatusCode: string;
  requiredPermission?: string;
  requiresReason: boolean;
  autoTriggerRule?: string;
}

export interface ConfigureTransitionsCommand {
  orgUnitId: number;
  transitions: ConfigureTransitionDefinition[];
}
