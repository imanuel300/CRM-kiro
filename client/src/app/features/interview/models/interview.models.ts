export enum InterviewType {
  First = 0,
  Second = 1,
}

export enum InterviewStatus {
  Scheduled = 0,
  Completed = 1,
  Cancelled = 2,
}

export interface Interview {
  id: number;
  orgUnitId: number;
  callForCandidatesId: number;
  candidacyId: number;
  scheduledDate: string;
  startTime: string; // "HH:MM:SS" from backend TimeSpan
  endTime: string;
  location?: string;
  interviewerIds: number[];
  interviewType: InterviewType;
  status: InterviewStatus;
  createdAt: string;
}

export interface InterviewFeedback {
  id: number;
  interviewId: number;
  interviewerId: number;
  rating: number;
  comments?: string;
  submittedAt: string;
}

export interface CreateInterviewCommand {
  orgUnitId: number;
  callForCandidatesId: number;
  candidacyId: number;
  scheduledDate: string;
  startTime: string;
  endTime: string;
  location?: string;
  interviewerIds: number[];
  interviewType: InterviewType;
}

export interface UpdateInterviewCommand {
  id: number;
  scheduledDate: string;
  startTime: string;
  endTime: string;
  location?: string;
  interviewerIds: number[];
}

export interface SubmitFeedbackCommand {
  interviewId: number;
  interviewerId: number;
  rating: number;
  comments?: string;
}

export interface InterviewQueryParams {
  orgUnitId?: number;
  callForCandidatesId?: number;
  candidacyId?: number;
}
