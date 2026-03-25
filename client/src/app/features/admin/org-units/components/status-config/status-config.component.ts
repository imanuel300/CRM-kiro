import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { OrgUnitService } from '../../services/org-unit.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsDropdownOption } from '@igds/angular';
import {
  CandidacyStatusCategory,
  ConfigureStatusDefinition,
  StatusDefinition,
} from '../../models/org-unit.models';

@Component({
  selector: 'app-status-config',
  template: `
    <div class="page-header">
      <h1>הגדרת סטטוסים</h1>
      <igds-button variant="primary" (onClick)="addStatus()">
        הוספת סטטוס
      </igds-button>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <form [formGroup]="form" (ngSubmit)="onSave()" *ngIf="!loading">
      <div formArrayName="statuses" class="status-list">
        <igds-card *ngFor="let status of statusControls.controls; let i = index" [formGroupName]="i" class="status-card">
          <div class="status-row">
            <igds-input-field
              class="field-code"
              label="קוד"
              formControlName="code"
              [required]="true"
              [error]="status.get('code')?.hasError('required') && status.get('code')?.touched ? 'חובה' : ''">
            </igds-input-field>

            <igds-input-field
              class="field-name"
              label="שם תצוגה"
              formControlName="displayName"
              [required]="true"
              [error]="status.get('displayName')?.hasError('required') && status.get('displayName')?.touched ? 'חובה' : ''">
            </igds-input-field>

            <igds-dropdown
              class="field-category"
              label="קטגוריה"
              formControlName="category"
              [options]="categoryOptions">
            </igds-dropdown>

            <igds-checkbox formControlName="isInitial" label="התחלתי"></igds-checkbox>
            <igds-checkbox formControlName="isFinal" label="סופי"></igds-checkbox>

            <igds-button variant="secondary" [iconOnly]="true" ariaLabel="הסרה" type="button" (onClick)="removeStatus(i)">
              <span igds-icon>🗑</span>
            </igds-button>
          </div>

          <!-- Sub-statuses -->
          <div formArrayName="subStatuses" class="sub-status-section" *ngIf="getSubStatuses(i).length > 0 || true">
            <div class="sub-header">
              <span>תתי סטטוס</span>
              <igds-button variant="secondary" [iconOnly]="true" ariaLabel="הוספת תת סטטוס" type="button" (onClick)="addSubStatus(i)">
                <span igds-icon>➕</span>
              </igds-button>
            </div>
            <div *ngFor="let sub of getSubStatuses(i).controls; let j = index" [formGroupName]="j" class="sub-status-row">
              <igds-input-field
                label="קוד"
                formControlName="code">
              </igds-input-field>
              <igds-input-field
                label="שם תצוגה"
                formControlName="displayName">
              </igds-input-field>
              <igds-button variant="secondary" [iconOnly]="true" ariaLabel="הסרת תת סטטוס" type="button" (onClick)="removeSubStatus(i, j)">
                <span igds-icon>➖</span>
              </igds-button>
            </div>
          </div>
        </igds-card>
      </div>

      <div class="form-actions">
        <igds-button variant="primary" type="submit" [disabled]="form.invalid || saving">
          {{ saving ? 'שומר...' : 'שמירה' }}
        </igds-button>
        <igds-button variant="secondary" type="button" routerLink="/admin/org-units">
          חזרה לרשימה
        </igds-button>
      </div>
    </form>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .status-list { display: flex; flex-direction: column; gap: var(--igds-space-12); }
    .status-card { margin-block-end: var(--igds-space-4); }
    .status-row {
      display: flex;
      align-items: center;
      gap: var(--igds-space-12);
      flex-wrap: wrap;
    }
    .field-code { width: 120px; }
    .field-name { flex: 1; min-width: 160px; }
    .field-category { width: 160px; }
    .sub-status-section {
      margin-block-start: var(--igds-space-8);
      padding-block-start: var(--igds-space-8);
      border-top: 1px dashed var(--igds-border-divider);
    }
    .sub-header {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      font-size: var(--igds-font-size-sm);
      font-family: var(--igds-font-family);
      color: var(--igds-text-secondary);
    }
    .sub-status-row {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      margin-inline-start: var(--igds-space-24);
    }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
  `],
})
export class StatusConfigComponent implements OnInit {
  form!: FormGroup;
  loading = false;
  saving = false;
  orgUnitId!: number;

  categories = [
    { value: CandidacyStatusCategory.Submitted, label: 'הוגשה' },
    { value: CandidacyStatusCategory.InReview, label: 'בבדיקה' },
    { value: CandidacyStatusCategory.Exam, label: 'מבחן' },
    { value: CandidacyStatusCategory.Interview, label: 'ראיון' },
    { value: CandidacyStatusCategory.Committee, label: 'ועדה' },
    { value: CandidacyStatusCategory.Accepted, label: 'התקבל' },
    { value: CandidacyStatusCategory.Rejected, label: 'נדחה' },
    { value: CandidacyStatusCategory.Withdrawn, label: 'נגרע' },
  ];

  categoryOptions: IgdsDropdownOption[] = this.categories.map(c => ({
    value: c.value,
    label: c.label,
  }));

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private orgUnitService: OrgUnitService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.orgUnitId = +this.route.snapshot.paramMap.get('id')!;
    this.form = this.fb.group({ statuses: this.fb.array([]) });
    this.loadStatuses();
  }

  get statusControls(): FormArray {
    return this.form.get('statuses') as FormArray;
  }

  getSubStatuses(statusIndex: number): FormArray {
    return this.statusControls.at(statusIndex).get('subStatuses') as FormArray;
  }

  addStatus(): void {
    this.statusControls.push(
      this.fb.group({
        code: ['', Validators.required],
        displayName: ['', Validators.required],
        category: [CandidacyStatusCategory.InReview],
        isInitial: [false],
        isFinal: [false],
        sortOrder: [this.statusControls.length + 1],
        subStatuses: this.fb.array([]),
      })
    );
  }

  removeStatus(index: number): void {
    this.statusControls.removeAt(index);
  }

  addSubStatus(statusIndex: number): void {
    this.getSubStatuses(statusIndex).push(
      this.fb.group({
        code: ['', Validators.required],
        displayName: ['', Validators.required],
      })
    );
  }

  removeSubStatus(statusIndex: number, subIndex: number): void {
    this.getSubStatuses(statusIndex).removeAt(subIndex);
  }

  private loadStatuses(): void {
    this.loading = true;
    this.orgUnitService.getStatuses(this.orgUnitId).subscribe({
      next: (statuses: StatusDefinition[]) => {
        statuses.forEach((s: StatusDefinition) => this.addStatusFromData(s));
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  private addStatusFromData(s: StatusDefinition): void {
    const subArray = this.fb.array(
      (s.subStatuses || []).map((sub) =>
        this.fb.group({ code: [sub.code, Validators.required], displayName: [sub.displayName, Validators.required] })
      )
    );
    this.statusControls.push(
      this.fb.group({
        code: [s.code, Validators.required],
        displayName: [s.displayName, Validators.required],
        category: [s.category],
        isInitial: [s.isInitial],
        isFinal: [s.isFinal],
        sortOrder: [s.sortOrder],
        subStatuses: subArray,
      })
    );
  }

  onSave(): void {
    if (this.form.invalid) return;
    this.saving = true;

    const statuses: ConfigureStatusDefinition[] = (this.statusControls.value as ConfigureStatusDefinition[]).map(
      (s, i: number) => ({
        code: s.code,
        displayName: s.displayName,
        category: s.category,
        isInitial: s.isInitial,
        isFinal: s.isFinal,
        sortOrder: i + 1,
        subStatuses: s.subStatuses?.length
          ? s.subStatuses.map((sub) => ({ code: sub.code, displayName: sub.displayName }))
          : undefined,
      })
    );

    this.orgUnitService.configureStatuses({ orgUnitId: this.orgUnitId, statuses }).subscribe({
      next: () => {
        this.notification.success('הסטטוסים עודכנו בהצלחה');
        this.saving = false;
      },
      error: () => {
        this.notification.error('שגיאה בשמירת הסטטוסים');
        this.saving = false;
      },
    });
  }
}
