export enum TenureEndReason {
  TermExpired = 'TermExpired',
  Resignation = 'Resignation',
  Termination = 'Termination',
}

export const TENURE_END_REASON_LABELS: { value: TenureEndReason; label: string }[] = [
  { value: TenureEndReason.TermExpired, label: 'סיום תקופה' },
  { value: TenureEndReason.Resignation, label: 'התפטרות' },
  { value: TenureEndReason.Termination, label: 'הפסקה' },
];

export interface Tenure {
  id: number;
  contactId: number;
  orgUnitId: number;
  position: string;
  startDate: string;
  expectedEndDate: string;
  actualEndDate?: string;
  endReason?: TenureEndReason;
  isActive: boolean;
  notes?: string;
  createdAt: string;
}

export interface CreateTenureCommand {
  contactId: number;
  orgUnitId: number;
  position: string;
  startDate: string;
  expectedEndDate: string;
  notes?: string;
}

export interface UpdateTenureCommand {
  id: number;
  position: string;
  startDate: string;
  expectedEndDate: string;
  notes?: string;
}

export interface EndTenureCommand {
  id: number;
  actualEndDate: string;
  endReason: TenureEndReason;
}

export interface ExpiringTenure {
  id: number;
  contactId: number;
  orgUnitId: number;
  position: string;
  expectedEndDate: string;
  daysUntilExpiry: number;
}
