export interface Exam {
  id: number;
  orgUnitId: number;
  callForCandidatesId: number;
  name: string;
  examDate: string;
  location?: string;
  maxScore: number;
  passingScore?: number;
  firstExaminerId?: number;
  secondExaminerId?: number;
  appealDeadline?: string;
}

export interface ExamScore {
  id: number;
  examId: number;
  candidacyId: number;
  firstExaminerScore?: number;
  secondExaminerScore?: number;
  finalScore?: number;
  scoreFormula?: string;
  passedThreshold?: boolean;
  isAppealed: boolean;
  appealScore?: number;
  scoredAt?: string;
}

export interface CreateExamCommand {
  orgUnitId: number;
  callForCandidatesId: number;
  name: string;
  examDate: string;
  location?: string;
  maxScore: number;
  passingScore?: number;
  firstExaminerId?: number;
  secondExaminerId?: number;
  appealDeadline?: string;
}

export interface UpdateExamCommand {
  id: number;
  name: string;
  examDate: string;
  location?: string;
  maxScore: number;
  passingScore?: number;
  firstExaminerId?: number;
  secondExaminerId?: number;
  appealDeadline?: string;
}

export interface SubmitScoreCommand {
  examId: number;
  candidacyId: number;
  firstExaminerScore?: number;
  secondExaminerScore?: number;
}

export interface SubmitAppealCommand {
  examId: number;
  candidacyId: number;
  appealScore: number;
  reason?: string;
}

export interface ExamQueryParams {
  orgUnitId?: number;
  callForCandidatesId?: number;
}
