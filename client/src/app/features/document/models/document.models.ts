export interface Document {
  id: number;
  candidacyId: number;
  documentType: string;
  fileName: string;
  blobUrl: string;
  contentType: string;
  sizeBytes: number;
  status: string;
  reviewedByUserId?: number;
  uploadedAt: string;
  version: number;
}

export interface DocumentVersion {
  id: number;
  fileName: string;
  blobUrl: string;
  sizeBytes: number;
  status: string;
  uploadedAt: string;
  version: number;
}

export interface RequiredDocument {
  id: number;
  callForCandidatesId?: number;
  orgUnitId?: number;
  documentType: string;
  isRequired: boolean;
  allowedFormats: string;
  maxSizeKB: number;
}

export interface UploadDocumentCommand {
  candidacyId: number;
  documentType: string;
  fileName: string;
  blobUrl: string;
  contentType: string;
  sizeBytes: number;
}

export interface ReviewDocumentCommand {
  documentId: number;
  status: 'Approved' | 'Rejected';
  reviewedByUserId: number;
}

export interface DocumentQueryParams {
  candidacyId?: number;
  documentType?: string;
  status?: string;
}

export interface DocumentCompletenessResult {
  isComplete: boolean;
  missingDocuments: MissingDocumentInfo[];
}

export interface MissingDocumentInfo {
  documentType: string;
  isRequired: boolean;
}

export type DocumentStatus = 'Missing' | 'Uploaded' | 'Approved' | 'Rejected';
