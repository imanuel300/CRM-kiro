export enum NotificationChannel {
  Email = 'Email',
  Sms = 'Sms',
}

export enum NotificationStatus {
  Sent = 'Sent',
  Failed = 'Failed',
  Pending = 'Pending',
}

export interface NotificationTemplate {
  id: number;
  orgUnitId: number;
  name: string;
  subject: string;
  body: string;
  channel: NotificationChannel;
  triggerEvent: string;
  isActive: boolean;
}

export interface NotificationLog {
  id: number;
  candidacyId: number;
  templateId: number;
  channel: NotificationChannel;
  recipient: string;
  subject: string;
  body: string;
  status: NotificationStatus;
  errorMessage?: string;
  sentAt: string;
}

export interface CreateTemplateCommand {
  orgUnitId: number;
  name: string;
  subject: string;
  body: string;
  channel: NotificationChannel;
  triggerEvent: string;
  isActive: boolean;
}

export interface UpdateTemplateCommand {
  id: number;
  name: string;
  subject: string;
  body: string;
  channel: NotificationChannel;
  triggerEvent: string;
  isActive: boolean;
}

export interface SendNotificationCommand {
  candidacyId: number;
  templateId: number;
  channel: NotificationChannel;
  recipient: string;
  variables?: Record<string, string>;
}

export interface SendBulkNotificationCommand {
  templateId: number;
  channel: NotificationChannel;
  recipients: BulkRecipient[];
  variables?: Record<string, string>;
}

export interface BulkRecipient {
  candidacyId: number;
  recipient: string;
  variables?: Record<string, string>;
}

export interface NotificationLogQueryParams {
  candidacyId?: number;
  channel?: NotificationChannel;
  status?: NotificationStatus;
  fromDate?: string;
  toDate?: string;
}

export const TRIGGER_EVENTS: { value: string; label: string }[] = [
  { value: 'StatusChanged', label: 'שינוי סטטוס' },
  { value: 'InterviewScheduled', label: 'זימון לראיון' },
  { value: 'ExamScheduled', label: 'זימון למבחן' },
  { value: 'CommitteeDecision', label: 'החלטת ועדה' },
  { value: 'DocumentRequired', label: 'דרישת מסמך' },
  { value: 'Manual', label: 'שליחה ידנית' },
];

export const TEMPLATE_VARIABLES: { key: string; label: string }[] = [
  { key: '{{שם_מועמד}}', label: 'שם המועמד' },
  { key: '{{סטטוס}}', label: 'סטטוס מועמדות' },
  { key: '{{תאריך}}', label: 'תאריך' },
  { key: '{{קול_קורא}}', label: 'שם קול קורא' },
  { key: '{{יחידה}}', label: 'שם יחידה ארגונית' },
];
