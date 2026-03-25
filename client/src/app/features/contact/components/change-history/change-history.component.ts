import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { ChangeHistory } from '../../models/contact.models';
import { ContactService } from '../../services/contact.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';

const FIELD_LABELS: Record<string, string> = {
  FirstName: 'שם פרטי',
  LastName: 'שם משפחה',
  DateOfBirth: 'תאריך לידה',
  Gender: 'מגדר',
  Address: 'כתובת',
  Phone: 'טלפון',
  Email: 'דוא"ל',
};

@Component({
  selector: 'app-change-history',
  template: `
    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div *ngIf="!loading && data.length === 0" class="no-data">
      אין היסטוריית שינויים
    </div>

    <igds-table
      *ngIf="!loading && data.length > 0"
      [columns]="columns"
      [data]="tableData">
    </igds-table>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
  `],
})
export class ChangeHistoryComponent implements OnChanges {
  @Input() contactId!: number;

  columns: IgdsTableColumn[] = [
    { key: 'changedAt', label: 'תאריך' },
    { key: 'fieldName', label: 'שדה' },
    { key: 'oldValue', label: 'ערך ישן' },
    { key: 'newValue', label: 'ערך חדש' },
    { key: 'changedByUserId', label: 'משתמש' },
  ];

  data: ChangeHistory[] = [];
  tableData: Record<string, any>[] = [];
  loading = false;

  constructor(
    private contactService: ContactService,
    private notification: NotificationService
  ) {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['contactId'] && this.contactId) {
      this.loadHistory();
    }
  }

  getFieldLabel(fieldName: string): string {
    return FIELD_LABELS[fieldName] || fieldName;
  }

  private loadHistory(): void {
    this.loading = true;
    this.contactService.getChangeHistory(this.contactId).subscribe({
      next: (data) => {
        this.data = data;
        this.tableData = data.map(row => ({
          changedAt: row.changedAt,
          fieldName: this.getFieldLabel(row.fieldName),
          oldValue: row.oldValue || '—',
          newValue: row.newValue || '—',
          changedByUserId: row.changedByUserId || '—',
        }));
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת היסטוריית שינויים');
        this.loading = false;
      },
    });
  }
}
