import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Contact,
  CreateContactCommand,
  UpdateContactCommand,
  ChangeHistory,
  CustomFieldValue,
  SetCustomFieldValueCommand,
} from '../models/contact.models';

@Injectable({ providedIn: 'root' })
export class ContactService {
  private readonly basePath = 'contacts';

  constructor(private api: ApiService) {}

  search(term?: string): Observable<Contact[]> {
    const params = term ? { searchTerm: term } : {};
    return this.api.get<Contact[]>(this.basePath, params);
  }

  getById(id: number): Observable<Contact> {
    return this.api.get<Contact>(`${this.basePath}/${id}`);
  }

  getByIdNumber(idNumber: string): Observable<Contact | null> {
    return this.api.get<Contact | null>(`${this.basePath}/by-id-number/${idNumber}`);
  }

  create(command: CreateContactCommand): Observable<Contact> {
    return this.api.post<Contact>(this.basePath, command);
  }

  update(command: UpdateContactCommand): Observable<Contact> {
    return this.api.put<Contact>(`${this.basePath}/${command.id}`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  getChangeHistory(contactId: number): Observable<ChangeHistory[]> {
    return this.api.get<ChangeHistory[]>(`${this.basePath}/${contactId}/history`);
  }

  getCustomFields(contactId: number, orgUnitId: number): Observable<CustomFieldValue[]> {
    return this.api.get<CustomFieldValue[]>(
      `${this.basePath}/${contactId}/custom-fields/${orgUnitId}`
    );
  }

  setCustomFieldValue(command: SetCustomFieldValueCommand): Observable<void> {
    return this.api.put<void>(
      `${this.basePath}/${command.contactId}/custom-fields/${command.orgUnitId}`,
      command
    );
  }
}
