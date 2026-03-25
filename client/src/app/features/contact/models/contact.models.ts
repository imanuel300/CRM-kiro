export interface Contact {
  id: number;
  idNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  phone?: string;
  email?: string;
  createdAt: string;
  updatedAt: string;
}

export interface CreateContactCommand {
  idNumber: string;
  firstName: string;
  lastName: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  phone?: string;
  email?: string;
}

export interface UpdateContactCommand {
  id: number;
  firstName: string;
  lastName: string;
  dateOfBirth?: string;
  gender?: string;
  address?: string;
  phone?: string;
  email?: string;
}

export interface ChangeHistory {
  id: number;
  fieldName: string;
  oldValue?: string;
  newValue?: string;
  changedByUserId?: number;
  changedAt: string;
}

export interface CustomFieldValue {
  id: number;
  customFieldDefinitionId: number;
  fieldName: string;
  fieldType: string;
  value?: string;
  orgUnitId: number;
}

export interface SetCustomFieldValueCommand {
  contactId: number;
  orgUnitId: number;
  customFieldDefinitionId: number;
  value?: string;
}
