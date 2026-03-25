// Sub-Unit models
export interface OrgSubUnit {
  id: number;
  orgUnitId: number;
  parentOrgSubUnitId: number | null;
  name: string;
  description?: string;
  isActive: boolean;
  createdAt: string;
}

export interface OrgSubUnitTree {
  id: number;
  orgUnitId: number;
  parentOrgSubUnitId: number | null;
  name: string;
  description?: string;
  isActive: boolean;
  children: OrgSubUnitTree[];
  positions: OrgPosition[];
}

export interface CreateSubUnitCommand {
  orgUnitId: number;
  parentOrgSubUnitId: number | null;
  name: string;
  description?: string;
}

export interface UpdateSubUnitCommand {
  id: number;
  name: string;
  description?: string;
  isActive: boolean;
}

// Position models
export interface OrgPosition {
  id: number;
  orgSubUnitId: number;
  title: string;
  maxOccupants: number;
  isActive: boolean;
  createdAt: string;
}

export interface CreatePositionCommand {
  orgSubUnitId: number;
  title: string;
  maxOccupants: number;
}

export interface UpdatePositionCommand {
  id: number;
  title: string;
  maxOccupants: number;
  isActive: boolean;
}

// Assignment models
export interface AssignToPositionCommand {
  orgPositionId: number;
  contactId: number;
  candidacyId: number;
  startDate: string;
}

export interface UnassignFromPositionCommand {
  assignmentId: number;
}

export interface PositionAssignment {
  id: number;
  orgPositionId: number;
  contactId: number;
  candidacyId: number;
  startDate: string;
  endDate?: string;
  isActive: boolean;
  createdAt: string;
}

// Occupancy models
export interface PositionOccupancy {
  positionId: number;
  title: string;
  maxOccupants: number;
  filledCount: number;
  vacantCount: number;
}

export interface SubUnitOccupancy {
  subUnitId: number;
  subUnitName: string;
  positions: PositionOccupancy[];
}
