export interface Quota {
  id: number;
  orgUnitId: number;
  categoryName: string;
  targetCount: number;
  currentCount: number;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateQuotaCommand {
  orgUnitId: number;
  categoryName: string;
  targetCount: number;
  description?: string;
}

export interface UpdateQuotaCommand {
  id: number;
  categoryName: string;
  targetCount: number;
  description?: string;
  isActive: boolean;
}

export interface AssignCandidacyCommand {
  quotaId: number;
  candidacyId: number;
}

export interface UnassignCandidacyCommand {
  quotaId: number;
  candidacyId: number;
}

export interface QuotaAssignment {
  id: number;
  quotaId: number;
  candidacyId: number;
  createdAt: string;
}

export interface QuotaFulfillment {
  quotaId: number;
  categoryName: string;
  targetCount: number;
  currentCount: number;
  fulfillmentPercentage: number;
  isActive: boolean;
}

export interface OrgUnitFulfillment {
  orgUnitId: number;
  quotas: QuotaFulfillment[];
}
