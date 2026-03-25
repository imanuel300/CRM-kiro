export enum MeetingStatus {
  Scheduled = 0,
  InProgress = 1,
  Completed = 2,
}

export enum CommitteeDecisionType {
  Accepted = 0,
  Rejected = 1,
  Deferred = 2,
}

export interface CommitteeMemberInfo {
  memberId: number;
  role: string;
}

export interface Committee {
  id: number;
  orgUnitId: number;
  name: string;
  description?: string;
  members: CommitteeMemberInfo[];
  createdAt: string;
}

export interface CommitteeMeeting {
  id: number;
  committeeId: number;
  orgUnitId: number;
  scheduledDate: string;
  location?: string;
  status: MeetingStatus;
  candidacyIds: number[];
  createdAt: string;
}

export interface CommitteeDecision {
  id: number;
  meetingId: number;
  candidacyId: number;
  decision: CommitteeDecisionType;
  recommendation?: string;
  decidedBy: number;
  decidedAt: string;
}

export interface CommitteeAppeal {
  id: number;
  meetingId: number;
  candidacyId: number;
  reason: string;
  result?: string;
  resolvedAt?: string;
  createdAt: string;
}

export interface CreateCommitteeCommand {
  orgUnitId: number;
  name: string;
  description?: string;
  members: CommitteeMemberInfo[];
}

export interface UpdateCommitteeCommand {
  id: number;
  name: string;
  description?: string;
  members: CommitteeMemberInfo[];
}

export interface CreateMeetingCommand {
  committeeId: number;
  orgUnitId: number;
  scheduledDate: string;
  location?: string;
  candidacyIds: number[];
}

export interface RecordDecisionCommand {
  meetingId: number;
  candidacyId: number;
  decision: CommitteeDecisionType;
  recommendation?: string;
  decidedBy: number;
}

export interface SubmitCommitteeAppealCommand {
  meetingId: number;
  candidacyId: number;
  reason: string;
}

export interface ResolveCommitteeAppealCommand {
  appealId: number;
  result: string;
}

export interface CommitteeQueryParams {
  orgUnitId?: number;
}
