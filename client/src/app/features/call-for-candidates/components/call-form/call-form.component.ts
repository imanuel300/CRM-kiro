import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { CallForCandidatesService } from '../../services/call-for-candidates.service';
import { NotificationService } from '@core/services/notification.service';
import {
  CallForCandidates,
  CallForCandidatesDetail,
  ThresholdCondition,
} from '../../models/call-for-candidates.models';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-call-form',
  template: `
    <div class="page-header">
      <h1>{{ isEdit ? 'עריכת קול קורא' : 'יצירת קול קורא חדש' }}</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <igds-card *ngIf="!loading">
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <h3>פרטים כלליים</h3>

        <igds-input-field
          label="כותרת"
          formControlName="title"
          [required]="true"
          [error]="form.get('title')?.touched && form.get('title')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-input-field>

        <igds-input-field
          label="תיאור"
          formControlName="description">
        </igds-input-field>

        <igds-input-field
          *ngIf="!isEdit"
          label="מזהה יחידה ארגונית"
          type="number"
          formControlName="orgUnitId"
          [required]="true"
          [error]="form.get('orgUnitId')?.touched && form.get('orgUnitId')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-input-field>

        <div class="date-row">
          <igds-date-picker class="date-field"
            label="תאריך פתיחה"
            formControlName="openDate"
            [required]="true"
            [error]="form.get('openDate')?.touched && form.get('openDate')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-date-picker>

          <igds-date-picker class="date-field"
            label="תאריך סגירה"
            formControlName="closeDate">
          </igds-date-picker>
        </div>

        <h3>הגדרות מכרז</h3>

        <igds-checkbox
          label="קול קורא מסוג מכרז"
          formControlName="isTender"
          class="tender-checkbox">
        </igds-checkbox>

        <div *ngIf="form.get('isTender')?.value" class="tender-fields">
          <igds-input-field
            label="ציון סף מינימלי"
            type="number"
            formControlName="minScore">
          </igds-input-field>

          <igds-input-field
            label="תנאי כשירות"
            formControlName="eligibilityConditions"
            placeholder="תנאי כשירות נוספים למכרז">
          </igds-input-field>
        </div>

        <h3>תנאי סף</h3>
        <div formArrayName="thresholdConditions">
          <div *ngFor="let cond of thresholdConditions.controls; let i = index"
               [formGroupName]="i" class="threshold-row">
            <igds-input-field class="threshold-field"
              label="שם שדה"
              formControlName="fieldName">
            </igds-input-field>
            <igds-dropdown class="threshold-field-sm"
              label="אופרטור"
              formControlName="operator"
              [options]="operatorOptions">
            </igds-dropdown>
            <igds-input-field class="threshold-field"
              label="ערך"
              formControlName="value">
            </igds-input-field>
            <igds-checkbox
              label="אוטומטי"
              formControlName="isAutomatic">
            </igds-checkbox>
            <igds-button variant="secondary" type="button" [iconOnly]="true"
              ariaLabel="הסרת תנאי"
              igdsTooltip="הסרת תנאי"
              (onClick)="removeThreshold(i)">
              <span igds-icon>✕</span>
            </igds-button>
          </div>
        </div>
        <igds-button variant="secondary" type="button" (onClick)="addThreshold()">
          הוספת תנאי סף
        </igds-button>

        <div class="form-actions">
          <igds-button variant="primary" type="submit"
                  [disabled]="form.invalid || saving">
            {{ saving ? 'שומר...' : (isEdit ? 'עדכון' : 'יצירה') }}
          </igds-button>
          <igds-button variant="secondary" type="button" (onClick)="onCancel()">ביטול</igds-button>
        </div>
      </form>
    </igds-card>
  `,
  styles: [`
    .page-header { margin-bottom: var(--igds-space-16); }
    :host ::ng-deep igds-input-field,
    :host ::ng-deep igds-dropdown,
    :host ::ng-deep igds-date-picker { width: 100%; margin-bottom: var(--igds-space-8); }
    .date-row { display: flex; gap: var(--igds-space-16); }
    .date-field { flex: 1; }
    .tender-checkbox { display: block; margin-bottom: var(--igds-space-16); }
    .tender-fields { margin-bottom: var(--igds-space-16); }
    .threshold-row {
      display: flex;
      gap: var(--igds-space-8);
      align-items: center;
      margin-bottom: var(--igds-space-4);
    }
    .threshold-field { flex: 2; }
    .threshold-field-sm { flex: 1; }
    .form-actions { display: flex; gap: var(--igds-space-8); margin-top: var(--igds-space-16); }
    h3 {
      margin-top: var(--igds-space-24);
      margin-bottom: var(--igds-space-12);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
  `],
})
export class CallFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  loading = false;
  saving = false;
  private editId?: number;

  operatorOptions: IgdsDropdownOption[] = [
    { value: '>=', label: '>=' },
    { value: '<=', label: '<=' },
    { value: '==', label: '=' },
    { value: '!=', label: '!=' },
    { value: '>', label: '>' },
    { value: '<', label: '<' },
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private callService: CallForCandidatesService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      orgUnitId: [null, Validators.required],
      title: ['', Validators.required],
      description: [''],
      openDate: [null, Validators.required],
      closeDate: [null],
      isTender: [false],
      minScore: [null],
      eligibilityConditions: [''],
      thresholdConditions: this.fb.array([]),
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.editId = +id;
      this.loadCall(this.editId);
    }
  }

  get thresholdConditions(): FormArray {
    return this.form.get('thresholdConditions') as FormArray;
  }

  addThreshold(): void {
    this.thresholdConditions.push(
      this.fb.group({
        fieldName: ['', Validators.required],
        operator: ['>=', Validators.required],
        value: ['', Validators.required],
        isAutomatic: [true],
      })
    );
  }

  removeThreshold(index: number): void {
    this.thresholdConditions.removeAt(index);
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const val = this.form.value;

    if (this.isEdit && this.editId) {
      const command = {
        id: this.editId,
        title: val.title,
        description: val.description || undefined,
        openDate: val.openDate instanceof Date ? val.openDate.toISOString() : val.openDate,
        closeDate: val.closeDate ? (val.closeDate instanceof Date ? val.closeDate.toISOString() : val.closeDate) : undefined,
        isTender: val.isTender,
        minScore: val.isTender ? val.minScore : undefined,
        eligibilityConditions: val.isTender ? val.eligibilityConditions : undefined,
      };
      this.callService.update(command).subscribe({
        next: () => {
          this.notification.success('הקול קורא עודכן בהצלחה');
          this.router.navigate(['/calls', this.editId]);
        },
        error: (err: any) => {
          this.notification.error(err?.error?.message || 'שגיאה בעדכון הקול קורא');
          this.saving = false;
        },
      });
    } else {
      const command = {
        orgUnitId: val.orgUnitId,
        title: val.title,
        description: val.description || undefined,
        openDate: val.openDate instanceof Date ? val.openDate.toISOString() : val.openDate,
        closeDate: val.closeDate ? (val.closeDate instanceof Date ? val.closeDate.toISOString() : val.closeDate) : undefined,
        isTender: val.isTender,
        minScore: val.isTender ? val.minScore : undefined,
        eligibilityConditions: val.isTender ? val.eligibilityConditions : undefined,
      };
      this.callService.create(command).subscribe({
        next: (created: CallForCandidates) => {
          this.notification.success('הקול קורא נוצר בהצלחה');
          this.saveThresholdConditions(created.id, val.thresholdConditions || []);
        },
        error: (err: any) => {
          this.notification.error(err?.error?.message || 'שגיאה ביצירת הקול קורא');
          this.saving = false;
        },
      });
    }
  }

  onCancel(): void {
    if (this.isEdit && this.editId) {
      this.router.navigate(['/calls', this.editId]);
    } else {
      this.router.navigate(['/calls']);
    }
  }

  private loadCall(id: number): void {
    this.loading = true;
    this.callService.getDetail(id).subscribe({
      next: (detail: CallForCandidatesDetail) => {
        this.form.patchValue({
          orgUnitId: detail.orgUnitId,
          title: detail.title,
          description: detail.description,
          openDate: detail.openDate ? new Date(detail.openDate) : null,
          closeDate: detail.closeDate ? new Date(detail.closeDate) : null,
          isTender: detail.isTender,
          minScore: detail.minScore,
          eligibilityConditions: detail.eligibilityConditions,
        });
        if (detail.thresholdConditions) {
          detail.thresholdConditions.forEach((tc: ThresholdCondition) => {
            this.thresholdConditions.push(
              this.fb.group({
                fieldName: [tc.fieldName, Validators.required],
                operator: [tc.operator, Validators.required],
                value: [tc.value, Validators.required],
                isAutomatic: [tc.isAutomatic],
              })
            );
          });
        }
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת פרטי הקול קורא');
        this.loading = false;
      },
    });
  }

  private saveThresholdConditions(callId: number, conditions: any[]): void {
    if (!conditions.length) {
      this.router.navigate(['/calls', callId]);
      return;
    }
    let completed = 0;
    conditions.forEach((cond) => {
      this.callService
        .addThresholdCondition({ callForCandidatesId: callId, ...cond })
        .subscribe({
          next: () => {
            completed++;
            if (completed === conditions.length) {
              this.router.navigate(['/calls', callId]);
            }
          },
          error: () => {
            this.notification.error('שגיאה בשמירת תנאי סף');
            completed++;
            if (completed === conditions.length) {
              this.router.navigate(['/calls', callId]);
            }
          },
        });
    });
  }
}
