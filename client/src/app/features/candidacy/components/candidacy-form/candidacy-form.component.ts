import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { CandidacyService } from '../../services/candidacy.service';
import { NotificationService } from '@core/services/notification.service';
import { Candidacy } from '../../models/candidacy.models';

@Component({
  selector: 'app-candidacy-form',
  template: `
    <div class="page-header">
      <h1>יצירת מועמדות חדשה</h1>
    </div>

    <igds-card>
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
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
          label="מזהה קול קורא"
          type="number"
          formControlName="callForCandidatesId"
          [required]="true"
          [error]="form.get('callForCandidatesId')?.touched && form.get('callForCandidatesId')?.hasError('required') ? 'שדה חובה' : ''">
        </igds-input-field>

        <div class="form-actions">
          <igds-button
            variant="primary"
            type="submit"
            [disabled]="form.invalid || saving">
            {{ saving ? 'שומר...' : 'יצירה' }}
          </igds-button>
          <igds-button variant="secondary" type="button" (onClick)="onCancel()">
            ביטול
          </igds-button>
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
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
  `],
})
export class CandidacyFormComponent implements OnInit {
  form!: FormGroup;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private candidacyService: CandidacyService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      contactId: [null, Validators.required],
      orgUnitId: [null, Validators.required],
      callForCandidatesId: [null, Validators.required],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;

    this.candidacyService.create(value).subscribe({
      next: (created: Candidacy) => {
        this.notification.success('המועמדות נוצרה בהצלחה');
        this.router.navigate(['/candidacies', created.id]);
      },
      error: (err: any) => {
        const msg = err?.error?.message || 'שגיאה ביצירת המועמדות';
        this.notification.error(msg);
        this.saving = false;
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/candidacies']);
  }
}
