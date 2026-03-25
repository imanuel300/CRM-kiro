import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from '@core/services/api.service';
import {
  NotificationTemplate,
  NotificationLog,
  CreateTemplateCommand,
  UpdateTemplateCommand,
  SendNotificationCommand,
  SendBulkNotificationCommand,
  NotificationLogQueryParams,
} from '../models/notification.models';

@Injectable({ providedIn: 'root' })
export class NotificationApiService {
  private readonly basePath = 'notifications';

  constructor(private api: ApiService) {}

  // --- Templates ---

  listTemplates(): Observable<NotificationTemplate[]> {
    return this.api.get<NotificationTemplate[]>(`${this.basePath}/templates`);
  }

  getTemplate(id: number): Observable<NotificationTemplate> {
    return this.api.get<NotificationTemplate>(`${this.basePath}/templates/${id}`);
  }

  createTemplate(command: CreateTemplateCommand): Observable<NotificationTemplate> {
    return this.api.post<NotificationTemplate>(`${this.basePath}/templates`, command);
  }

  updateTemplate(command: UpdateTemplateCommand): Observable<NotificationTemplate> {
    return this.api.put<NotificationTemplate>(`${this.basePath}/templates/${command.id}`, command);
  }

  deleteTemplate(id: number): Observable<void> {
    return this.api.delete<void>(`${this.basePath}/templates/${id}`);
  }

  // --- Send ---

  send(command: SendNotificationCommand): Observable<NotificationLog> {
    return this.api.post<NotificationLog>(`${this.basePath}/send`, command);
  }

  sendBulk(command: SendBulkNotificationCommand): Observable<NotificationLog[]> {
    return this.api.post<NotificationLog[]>(`${this.basePath}/send-bulk`, command);
  }

  // --- Logs ---

  getLogs(params?: NotificationLogQueryParams): Observable<NotificationLog[]> {
    const queryParams: Record<string, string | number | boolean> = {};
    if (params?.candidacyId) queryParams['candidacyId'] = params.candidacyId;
    if (params?.channel) queryParams['channel'] = params.channel;
    if (params?.status) queryParams['status'] = params.status;
    if (params?.fromDate) queryParams['fromDate'] = params.fromDate;
    if (params?.toDate) queryParams['toDate'] = params.toDate;
    return this.api.get<NotificationLog[]>(`${this.basePath}/logs`, queryParams);
  }
}
