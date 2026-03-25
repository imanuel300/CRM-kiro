import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  Tenure,
  TenureEndReason,
  TENURE_END_REASON_LABELS,
} from '../../models/tenure.models';
import { TenureApiService } from '../../services/tenure.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-tenure-form',
  template: `
    <div class="page-header">
      <h1>{{ pageTitle }}</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <igds-card *ngIf="!loading">
      <!-- Create / Edit form -->
      <form *ngIf="!isEndMode" [formGroup]="form" (ngSubmit)="onSubmit()">
        <igds-input-field
          label="מזהה איש קשר"
          type="number"
          formControlName="contactId"
          [required]="true"
          [error]="form.get('contactId')?.touched && form.get('contactId')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-input-field>

        <igds-input-field
          label="מזהה יחידה ארגונית"
          type="number"
          formControlName="orgUnitId"
          [required]="true"
          [error]="form.get('orgUnitId')?.touched && form.get('orgUnitId')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-input-field>

        <igds-input-field
          label="תפקיד"
          formControlName="position"
          [required]="true"
          [error]="form.get('position')?.touched && form.get('position')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-input-field>

        <div class="form-row">
          <igds-date-picker class="form-field"
            label="תאריך התחלה"
            formControlName="startDate"
            [required]="true"
            [error]="form.get('startDate')?.touched && form.get('startDate')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-date-picker>

          <igds-date-picker class="form-field"
            label="תאריך סיום צפוי"
            formControlName="expectedEndDate"
            [required]="true"
            [error]="form.get('expectedEndDate')?.touched && form.get('expectedEndDate')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-date-picker>
        </div>

        <igds-input-field
          label="הערות"
          formControlName="notes">
        </igds-input-field>

        <div class="form-actions">
          <igds-button variant="primary" type="submit"
                  [disabled]="form.invalid || saving">
            {{ isEdit ? 'עדכון' : 'יצירה' }}
          </igds-button>
          <igds-button variant="secondary" type="button" (onClick)="onCancel()">ביטול</igds-button>
        </div>
      </form>

      <!-- End tenure form -->
      <form *ngIf="isEndMode" [formGroup]="endForm" (ngSubmit)="onEndSubmit()">
        <p class="end-info">סיום כהונה: <strong>{{ existingTenure?.position }}</strong></p>

        <igds-date-picker
          label="תאריך סיום בפועל"
          formControlName="actualEndDate"
          [required]="true"
          [error]="endForm.get('actualEndDate')?.touched && endForm.get('actualEndDate')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-date-picker>

        <igds-dropdown
          label="סיבת סיום"
          formControlName="endReason"
          [options]="endReasonDropdownOptions"
          [required]="true"
          [error]="endForm.get('endReason')?.touched && endForm.get('endReason')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-dropdown>

        <div class="form-actions">
          <igds-button variant="primary" type="submit"
                  [disabled]="endForm.invalid || saving">
            סיום כהונה
          </igds-button>
          <igds-button variant="secondary" type="button" (onClick)="onCancel()">ביטול</igds-button>
        </div>
      </form>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    :host ::ng-deep igds-input-field,
    :host ::ng-deep igds-dropdown,
    :host ::ng-deep igds-date-picker { width: 100%; margin-bottom: var(--igds-space-8); }
    .form-row { display: flex; gap: var(--igds-space-16); flex-wrap: wrap; }
    .form-field { flex: 1; min-width: 200px; }
    .form-actions { display: flex; gap: var(--igds-space-8); margin-block-start: var(--igds-space-16); }
    .end-info {
      font-size: var(--igds-font-size-md);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
      margin-block-end: var(--igds-space-16);
    }
  `],
})
export class TenureFormComponent implements OnInit {
  form!: FormGroup;
  endForm!: FormGroup;
  isEdit = false;
  isEndMode = false;
  loading = false;
  saving = false;
  tenureId?: number;
  existingTenure?: Tenure;
  endReasonOptions = TENURE_END_REASON_LABELS;

  endReasonDropdownOptions: IgdsDropdownOption[] = TENURE_END_REASON_LABELS.map(r => ({
    value: r.value,
    label: r.label,
  }));

  get pageTitle(): string {
    if (this.isEndMode) return 'סיום כהונה';
    return this.isEdit ? 'עריכת כהונה' : 'כהונה חדשה';
  }

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private tenureApi: TenureApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      contactId: [null, Validators.required],
      orgUnitId: [null, Validators.required],
      position: ['', Validators.required],
      startDate: [null, Validators.required],
      expectedEndDate: [null, Validators.required],
      notes: [''],
    });

    this.endForm = this.fb.group({
      actualEndDate: [null, Validators.required],
      endReason: [null, Validators.required],
    });

    const id = this.route.snapshot.paramMap.get('id');
    const url = this.route.snapshot.url.map((s: { path: string }) => s.path);

    if (id && id !== 'new') {
      this.tenureId = +id;
      this.isEndMode = url.includes('end');
      this.isEdit = !this.isEndMode;
      this.loadTenure(this.tenureId);
    }
  }

  loadTenure(id: number): void {
    this.loading = true;
    this.tenureApi.getById(id).subscribe({
      next: (tenure: Tenure) => {
        this.existingTenure = tenure;
        if (!this.isEndMode) {
          this.form.patchValue({
            contactId: tenure.contactId,
            orgUnitId: tenure.orgUnitId,
            position: tenure.position,
            startDate: tenure.startDate,
            expectedEndDate: tenure.expectedEndDate,
            notes: tenure.notes,
          });
        }
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת הכהונה');
        this.loading = false;
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const v = this.form.value;
    const formatDate = (d: any): string => d instanceof Date ? d.toISOString() : d;

    if (this.isEdit && this.tenureId) {
      this.tenureApi.update({
        id: this.tenureId,
        position: v.position,
        startDate: formatDate(v.startDate),
        expectedEndDate: formatDate(v.expectedEndDate),
        notes: v.notes,
      }).subscribe({
        next: () => {
          this.notification.success('הכהונה עודכנה בהצלחה');
          this.router.navigate(['/tenures']);
        },
        error: () => {
          this.notification.error('שגיאה בעדכון הכהונה');
          this.saving = false;
        },
      });
    } else {
      this.tenureApi.create({
        contactId: v.contactId,
        orgUnitId: v.orgUnitId,
        position: v.position,
        startDate: formatDate(v.startDate),
        expectedEndDate: formatDate(v.expectedEndDate),
        notes: v.notes,
      }).subscribe({
        next: () => {
          this.notification.success('הכהונה נוצרה בהצלחה');
          this.router.navigate(['/tenures']);
        },
        error: () => {
          this.notification.error('שגיאה ביצירת הכהונה');
          this.saving = false;
        },
      });
    }
  }

  onEndSubmit(): void {
    if (this.endForm.invalid || !this.tenureId) return;
    this.saving = true;
    const v = this.endForm.value;
    const formatDate = (d: any): string => d instanceof Date ? d.toISOString() : d;

    this.tenureApi.endTenure({
      id: this.tenureId,
      actualEndDate: formatDate(v.actualEndDate),
      endReason: v.endReason,
    }).subscribe({
      next: () => {
        this.notification.success('הכהונה סויימה בהצלחה');
        this.router.navigate(['/tenures']);
      },
      error: () => {
        this.notification.error('שגיאה בסיום הכהונה');
        this.saving = false;
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/tenures']);
  }
}
