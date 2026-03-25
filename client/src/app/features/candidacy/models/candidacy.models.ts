export interface Candidacy {
  id: number;
  contactId: number;
  orgUnitId: number;
  callForCandidatesId: number;
  currentStatusId?: number;
  currentSubStatusId?: number;
  workflowDefinitionVersion?: number;
  isActive: boolean;
  submittedAt?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CandidacyDetail extends Candidacy {
  customFields: CandidacyCustomFieldValue[];
}

export interface CandidacyCustomFieldValue {
  id: number;
  customFieldDefinitionId: number;
  fieldName: string;
  fieldType: string;
  value?: string;
}

export interface CreateCandidacyCommand {
  contactId: number;
  orgUnitId: number;
  callForCandidatesId: number;
}

export interface UpdateCandidacyCommand {
  id: number;
  currentSubStatusId?: number;
}

export interface SetCandidacyCustomFieldValueCommand {
  candidacyId: number;
  customFieldDefinitionId: number;
  value?: string;
}

export interface CandidacyQueryParams {
  orgUnitId?: number;
  contactId?: number;
  callForCandidatesId?: number;
  isActive?: boolean;
}

export interface TransitionStatusCommand {
  candidacyId: number;
  newStatusId: number;
  reason?: string;
  userId: number;
}

export interface StatusHistory {
  id: number;
  candidacyId: number;
  fromStatusId?: number;
  toStatusId: number;
  fromSubStatusId?: number;
  toSubStatusId?: number;
  reason?: string;
  changedByUserId: number;
  changedAt: string;
}
