import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  Tenure,
  CreateTenureCommand,
  UpdateTenureCommand,
  EndTenureCommand,
  ExpiringTenure,
} from '../models/tenure.models';

@Injectable({ providedIn: 'root' })
export class TenureApiService {
  private readonly basePath = 'tenures';

  constructor(private api: ApiService) {}

  getById(id: number): Observable<Tenure> {
    return this.api.get<Tenure>(`${this.basePath}/${id}`);
  }

  create(command: CreateTenureCommand): Observable<Tenure> {
    return this.api.post<Tenure>(this.basePath, command);
  }

  update(command: UpdateTenureCommand): Observable<Tenure> {
    return this.api.put<Tenure>(`${this.basePath}/${command.id}`, command);
  }

  endTenure(command: EndTenureCommand): Observable<Tenure> {
    return this.api.put<Tenure>(`${this.basePath}/${command.id}/end`, command);
  }

  delete(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/${id}`);
  }

  getByContact(contactId: number): Observable<Tenure[]> {
    return this.api.get<Tenure[]>(`${this.basePath}/by-contact/${contactId}`);
  }

  getByOrgUnit(orgUnitId: number): Observable<Tenure[]> {
    return this.api.get<Tenure[]>(`${this.basePath}/by-org-unit/${orgUnitId}`);
  }

  getExpiring(daysAhead: number = 30): Observable<ExpiringTenure[]> {
    return this.api.get<ExpiringTenure[]>(`${this.basePath}/expiring`, { daysAhead });
  }

  getHistory(contactId: number): Observable<Tenure[]> {
    return this.api.get<Tenure[]>(`${this.basePath}/history/${contactId}`);
  }
}
