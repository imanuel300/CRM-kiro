export interface ConflictOfInterest {
  id: number;
  candidacyId: number;
  contactId: number;
  questionnaireResponses: string;
  hasConflict: boolean;
  requiresManualReview: boolean;
  reviewedByUserId?: number;
  reviewedAt?: string;
}

export interface FamilyRelation {
  id: number;
  candidacyId: number;
  contactId: number;
  relationType: string;
  relatedPersonName: string;
  relatedPersonRole?: string;
  requiresManualReview: boolean;
}

export interface CandidacyDeclarations {
  candidacyId: number;
  conflictsOfInterest: ConflictOfInterest[];
  familyRelations: FamilyRelation[];
}

export interface CreateConflictCommand {
  candidacyId: number;
  contactId: number;
  questionnaireResponses: string;
  hasConflict: boolean;
}

export interface UpdateConflictCommand {
  id: number;
  questionnaireResponses: string;
  hasConflict: boolean;
}

export interface CreateFamilyRelationCommand {
  candidacyId: number;
  contactId: number;
  relationType: string;
  relatedPersonName: string;
  relatedPersonRole?: string;
}

export interface UpdateFamilyRelationCommand {
  id: number;
  relationType: string;
  relatedPersonName: string;
  relatedPersonRole?: string;
}

export interface ReviewConflictCommand {
  id: number;
  reviewedByUserId: number;
}

export const RELATION_TYPES: { value: string; label: string }[] = [
  { value: 'Parent', label: 'הורה' },
  { value: 'Child', label: 'ילד/ה' },
  { value: 'Spouse', label: 'בן/בת זוג' },
  { value: 'Sibling', label: 'אח/אחות' },
  { value: 'Other', label: 'אחר' },
];
