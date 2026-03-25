import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  CallForCandidatesDetail,
  ThresholdCondition,
  Position,
  ClosingSummary,
} from '../../models/call-for-candidates.models';
import { CallForCandidatesService } from '../../services/call-for-candidates.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';
import { IgdsTab } from '@igds/angular';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-call-detail',
  template: `
    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div *ngIf="!loading && call">
      <div class="page-header">
        <h1>{{ call.title }}</h1>
        <div class="header-actions">
          <igds-button variant="primary" (onClick)="onEdit()">עריכה</igds-button>
          <igds-button variant="secondary" (onClick)="onBack()">חזרה לרשימה</igds-button>
        </div>
      </div>

      <div class="badges">
        <igds-tag [label]="call.isActive ? 'פעיל' : 'לא פעיל'"
                  [variant]="call.isActive ? 'success' : 'default'"></igds-tag>
        <igds-tag *ngIf="call.isTender" label="מכרז" variant="default"></igds-tag>
      </div>

      <igds-tabs [tabs]="detailTabs" [activeTab]="activeTab" (tabChange)="activeTab = $event"></igds-tabs>

      <!-- General Details Tab -->
      <div *ngIf="activeTab === 'general'" class="tab-panel" role="tabpanel" [attr.aria-labelledby]="'tab-general'">
        <igds-card>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="label">מזהה</span>
              <span class="value">{{ call.id }}</span>
            </div>
            <div class="detail-item">
              <span class="label">יחידה ארגונית</span>
              <span class="value">{{ call.orgUnitId }}</span>
            </div>
            <div class="detail-item">
              <span class="label">תאריך פתיחה</span>
              <span class="value">{{ call.openDate | hebrewDate }}</span>
            </div>
            <div class="detail-item">
              <span class="label">תאריך סגירה</span>
              <span class="value">{{ call.closeDate ? (call.closeDate | hebrewDate) : 'לא הוגדר' }}</span>
            </div>
            <div class="detail-item" *ngIf="call.description">
              <span class="label">תיאור</span>
              <span class="value">{{ call.description }}</span>
            </div>
            <div class="detail-item">
              <span class="label">נוצר בתאריך</span>
              <span class="value">{{ call.createdAt | hebrewDate }}</span>
            </div>
          </div>

          <div *ngIf="call.isTender" class="tender-section">
            <h3>פרטי מכרז</h3>
            <div class="detail-grid">
              <div class="detail-item">
                <span class="label">ציון סף מינימלי</span>
                <span class="value">{{ call.minScore ?? 'לא הוגדר' }}</span>
              </div>
              <div class="detail-item" *ngIf="call.eligibilityConditions">
                <span class="label">תנאי כשירות</span>
                <span class="value">{{ call.eligibilityConditions }}</span>
              </div>
            </div>
          </div>
        </igds-card>
      </div>

      <!-- Threshold Conditions Tab -->
      <div *ngIf="activeTab === 'thresholds'" class="tab-panel" role="tabpanel" [attr.aria-labelledby]="'tab-thresholds'">
        <igds-card>
          <igds-table *ngIf="call.thresholdConditions?.length; else noThresholds"
            [columns]="thresholdTableColumns"
            [data]="thresholdTableData">
          </igds-table>
          <div *ngIf="call.thresholdConditions?.length" class="table-actions">
            <div *ngFor="let row of call.thresholdConditions" class="action-row">
              <igds-button variant="secondary" [iconOnly]="true"
                ariaLabel="הסרה"
                igdsTooltip="הסרה"
                (onClick)="removeThreshold(row)">
                <span igds-icon>✕</span>
              </igds-button>
            </div>
          </div>
          <ng-template #noThresholds>
            <p class="empty-message">לא הוגדרו תנאי סף</p>
          </ng-template>

          <h3>הוספת תנאי סף</h3>
          <form [formGroup]="thresholdForm" (ngSubmit)="addThreshold()" class="add-form">
            <igds-input-field label="שם שדה" formControlName="fieldName"></igds-input-field>
            <igds-dropdown label="אופרטור" formControlName="operator"
              [options]="operatorOptions"></igds-dropdown>
            <igds-input-field label="ערך" formControlName="value"></igds-input-field>
            <igds-checkbox label="אוטומטי" formControlName="isAutomatic"></igds-checkbox>
            <igds-button variant="primary" type="submit" [disabled]="thresholdForm.invalid">
              הוספה
            </igds-button>
          </form>
        </igds-card>
      </div>

      <!-- Positions Tab -->
      <div *ngIf="activeTab === 'positions'" class="tab-panel" role="tabpanel" [attr.aria-labelledby]="'tab-positions'">
        <igds-card>
          <igds-table *ngIf="call.positions?.length; else noPositions"
            [columns]="positionTableColumns"
            [data]="positionTableData">
          </igds-table>
          <div *ngIf="call.positions?.length" class="table-actions">
            <div *ngFor="let row of call.positions" class="action-row">
              <igds-button variant="secondary" [iconOnly]="true"
                ariaLabel="הסרה"
                igdsTooltip="הסרה"
                (onClick)="removePosition(row)">
                <span igds-icon>✕</span>
              </igds-button>
            </div>
          </div>
          <ng-template #noPositions>
            <p class="empty-message">לא הוגדרו תפקידים</p>
          </ng-template>

          <h3>הוספת תפקיד</h3>
          <form [formGroup]="positionForm" (ngSubmit)="addPosition()" class="add-form">
            <igds-input-field label="כותרת" formControlName="title"></igds-input-field>
            <igds-input-field label="תיאור" formControlName="description"></igds-input-field>
            <igds-input-field label="מספר משרות" type="number" formControlName="vacancies"></igds-input-field>
            <igds-button variant="primary" type="submit" [disabled]="positionForm.invalid">
              הוספה
            </igds-button>
          </form>
        </igds-card>
      </div>

      <!-- Closing Summary Tab -->
      <div *ngIf="activeTab === 'summary'" class="tab-panel" role="tabpanel" [attr.aria-labelledby]="'tab-summary'">
        <igds-card>
          <app-loading-spinner [loading]="loadingSummary"></app-loading-spinner>
          <div *ngIf="!loadingSummary && closingSummary" class="summary-grid">
            <igds-card>
              <div class="summary-number">{{ closingSummary.totalCandidacies }}</div>
              <div class="summary-label">סה"כ מועמדויות</div>
            </igds-card>
            <igds-card class="summary-card--success">
              <div class="summary-number">{{ closingSummary.metThreshold }}</div>
              <div class="summary-label">עמדו בתנאי סף</div>
            </igds-card>
            <igds-card class="summary-card--warn">
              <div class="summary-number">{{ closingSummary.rejected }}</div>
              <div class="summary-label">נדחו</div>
            </igds-card>
          </div>
          <div *ngIf="!loadingSummary && !closingSummary" class="empty-message">
            <p>סיכום סגירה יהיה זמין לאחר סגירת הקול קורא</p>
          </div>
        </igds-card>
      </div>
    </div>
  `,
  styles: [`
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: var(--igds-space-8);
      font-family: var(--igds-font-family);
    }
    .header-actions { display: flex; gap: var(--igds-space-8); }
    .badges { display: flex; gap: var(--igds-space-8); margin-bottom: var(--igds-space-16); }
    .tab-panel { margin-top: var(--igds-space-16); }
    .detail-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--igds-space-16);
    }
    .detail-item { display: flex; flex-direction: column; }
    .label {
      font-size: var(--igds-font-size-xs);
      color: var(--igds-text-secondary);
      margin-bottom: var(--igds-space-4);
      font-family: var(--igds-font-family);
    }
    .value {
      font-size: var(--igds-font-size-md);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .tender-section { margin-top: var(--igds-space-24); }
    .empty-message {
      color: var(--igds-text-secondary);
      text-align: center;
      padding: var(--igds-space-16);
      font-family: var(--igds-font-family);
    }
    h3 {
      margin-top: var(--igds-space-24);
      margin-bottom: var(--igds-space-12);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .add-form {
      display: flex;
      gap: var(--igds-space-8);
      align-items: center;
      flex-wrap: wrap;
    }
    .table-actions { display: none; }
    .summary-grid {
      display: grid;
      grid-template-columns: repeat(3, 1fr);
      gap: var(--igds-space-16);
    }
    .summary-card--success {
      border-inline-start: 4px solid var(--igds-border-success);
    }
    .summary-card--warn {
      border-inline-start: 4px solid var(--igds-border-failure);
    }
    .summary-number {
      font-size: 36px;
      font-weight: var(--igds-font-weight-bold, 700);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
      text-align: center;
    }
    .summary-label {
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-secondary);
      margin-top: var(--igds-space-4);
      font-family: var(--igds-font-family);
      text-align: center;
    }
  `],
})
export class CallDetailComponent implements OnInit {
  call: CallForCandidatesDetail | null = null;
  closingSummary: ClosingSummary | null = null;
  loading = false;
  loadingSummary = false;

  activeTab = 'general';

  detailTabs: IgdsTab[] = [
    { id: 'general', label: 'פרטים כלליים' },
    { id: 'thresholds', label: 'תנאי סף' },
    { id: 'positions', label: 'תפקידים/משרות' },
    { id: 'summary', label: 'סיכום סגירה' },
  ];

  thresholdTableColumns: IgdsTableColumn[] = [
    { key: 'fieldName', label: 'שם שדה' },
    { key: 'operator', label: 'אופרטור' },
    { key: 'value', label: 'ערך' },
    { key: 'isAutomatic', label: 'אוטומטי' },
  ];

  positionTableColumns: IgdsTableColumn[] = [
    { key: 'title', label: 'כותרת' },
    { key: 'description', label: 'תיאור' },
    { key: 'vacancies', label: 'משרות' },
  ];

  operatorOptions: IgdsDropdownOption[] = [
    { value: '>=', label: '>=' },
    { value: '<=', label: '<=' },
    { value: '==', label: '=' },
    { value: '!=', label: '!=' },
  ];

  thresholdForm!: FormGroup;
  positionForm!: FormGroup;

  private callId!: number;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private callService: CallForCandidatesService,
    private notification: NotificationService
  ) {}

  get thresholdTableData(): any[] {
    if (!this.call?.thresholdConditions) return [];
    return this.call.thresholdConditions.map(tc => ({
      ...tc,
      isAutomatic: tc.isAutomatic ? '✓' : '✗',
      description: tc.value,
    }));
  }

  get positionTableData(): any[] {
    if (!this.call?.positions) return [];
    return this.call.positions.map(p => ({
      ...p,
      description: p.description || '—',
    }));
  }

  ngOnInit(): void {
    this.thresholdForm = this.fb.group({
      fieldName: ['', Validators.required],
      operator: ['>=', Validators.required],
      value: ['', Validators.required],
      isAutomatic: [true],
    });

    this.positionForm = this.fb.group({
      title: ['', Validators.required],
      description: [''],
      vacancies: [1, [Validators.required, Validators.min(1)]],
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.callId = +id;
      this.loadCall(this.callId);
    }
  }

  onBack(): void {
    this.router.navigate(['/calls']);
  }

  onEdit(): void {
    this.router.navigate(['/calls', this.callId, 'edit']);
  }

  addThreshold(): void {
    if (this.thresholdForm.invalid) return;
    const val = this.thresholdForm.value;
    this.callService
      .addThresholdCondition({ callForCandidatesId: this.callId, ...val })
      .subscribe({
        next: () => {
          this.notification.success('תנאי סף נוסף בהצלחה');
          this.thresholdForm.reset({ operator: '>=', isAutomatic: true });
          this.loadCall(this.callId);
        },
        error: () => this.notification.error('שגיאה בהוספת תנאי סף'),
      });
  }

  removeThreshold(condition: ThresholdCondition): void {
    this.callService
      .removeThresholdCondition(this.callId, condition.id)
      .subscribe({
        next: () => {
          this.notification.success('תנאי הסף הוסר');
          this.loadCall(this.callId);
        },
        error: () => this.notification.error('שגיאה בהסרת תנאי סף'),
      });
  }

  addPosition(): void {
    if (this.positionForm.invalid) return;
    const val = this.positionForm.value;
    this.callService
      .addPosition({ callForCandidatesId: this.callId, ...val })
      .subscribe({
        next: () => {
          this.notification.success('תפקיד נוסף בהצלחה');
          this.positionForm.reset({ vacancies: 1 });
          this.loadCall(this.callId);
        },
        error: () => this.notification.error('שגיאה בהוספת תפקיד'),
      });
  }

  removePosition(position: Position): void {
    this.callService
      .removePosition(this.callId, position.id)
      .subscribe({
        next: () => {
          this.notification.success('התפקיד הוסר');
          this.loadCall(this.callId);
        },
        error: () => this.notification.error('שגיאה בהסרת תפקיד'),
      });
  }

  private loadCall(id: number): void {
    this.loading = true;
    this.callService.getDetail(id).subscribe({
      next: (detail: CallForCandidatesDetail) => {
        this.call = detail;
        this.loading = false;
        this.loadClosingSummary(id);
      },
      error: () => {
        this.notification.error('שגיאה בטעינת פרטי הקול קורא');
        this.loading = false;
      },
    });
  }

  private loadClosingSummary(id: number): void {
    if (!this.call || this.call.isActive) {
      this.closingSummary = null;
      return;
    }
    this.loadingSummary = true;
    this.callService.getClosingSummary(id).subscribe({
      next: (summary: ClosingSummary) => {
        this.closingSummary = summary;
        this.loadingSummary = false;
      },
      error: () => {
        this.closingSummary = null;
        this.loadingSummary = false;
      },
    });
  }
}
