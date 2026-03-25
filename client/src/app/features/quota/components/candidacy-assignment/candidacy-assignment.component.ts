import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { QuotaApiService } from '../../services/quota.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-candidacy-assignment',
  template: `
    <div class="page-header">
      <h1>שיוך מועמדות למכסה</h1>
    </div>

    <igds-card>
      <div igds-card-body>
        <form [formGroup]="form" (ngSubmit)="onAssign()">
          <igds-input-field
            label="מזהה מכסה"
            type="number"
            formControlName="quotaId"
            [required]="true"
            [error]="form.get('quotaId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="מזהה מועמדות"
            type="number"
            formControlName="candidacyId"
            [required]="true"
            [error]="form.get('candidacyId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <div class="form-actions">
            <igds-button variant="primary" type="submit"
                         [disabled]="form.invalid || saving">
              שיוך למכסה
            </igds-button>
            <igds-button variant="primary" type="button"
                         [disabled]="form.invalid || saving"
                         (onClick)="onUnassign()">
              הסרת שיוך
            </igds-button>
          </div>
        </form>
      </div>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
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
export class CandidacyAssignmentComponent {
  form: FormGroup;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private quotaApi: QuotaApiService,
    private notification: NotificationService
  ) {
    this.form = this.fb.group({
      quotaId: [null, Validators.required],
      candidacyId: [null, Validators.required],
    });
  }

  onAssign(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const { quotaId, candidacyId } = this.form.value;

    this.quotaApi.assignCandidacy({ quotaId, candidacyId }).subscribe({
      next: () => {
        this.notification.success('המועמדות שויכה למכסה בהצלחה');
        this.saving = false;
        this.form.reset();
      },
      error: () => {
        this.notification.error('שגיאה בשיוך מועמדות למכסה');
        this.saving = false;
      },
    });
  }

  onUnassign(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const { quotaId, candidacyId } = this.form.value;

    this.quotaApi.unassignCandidacy({ quotaId, candidacyId }).subscribe({
      next: () => {
        this.notification.success('שיוך המועמדות הוסר בהצלחה');
        this.saving = false;
        this.form.reset();
      },
      error: () => {
        this.notification.error('שגיאה בהסרת שיוך מועמדות');
        this.saving = false;
      },
    });
  }
}
