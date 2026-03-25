import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ThresholdCheckService } from '../../services/threshold-check.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-manual-check-form',
  template: `
    <div class="page-header">
      <h1>בדיקת תנאי סף ידנית</h1>
    </div>

    <igds-card>
      <div igds-card-body>
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <igds-input-field
            label="מזהה תנאי סף"
            type="number"
            formControlName="conditionId"
            [required]="true"
            [error]="form.get('conditionId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-radio-button
            formControlName="passed"
            label="תוצאה"
            [options]="[{value: true, label: 'עבר'}, {value: false, label: 'לא עבר'}]">
          </igds-radio-button>

          <igds-input-field
            label="הערות"
            formControlName="notes"
            placeholder="הערות לבדיקה הידנית">
          </igds-input-field>

          <div class="form-actions">
            <igds-button variant="primary" type="submit"
                         [disabled]="form.invalid || saving">
              שמירה
            </igds-button>
            <igds-button variant="secondary" type="button" (onClick)="onCancel()">
              ביטול
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
export class ManualCheckFormComponent implements OnInit {
  candidacyId!: number;
  saving = false;
  form!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private thresholdService: ThresholdCheckService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.candidacyId = Number(this.route.snapshot.paramMap.get('candidacyId'));

    const conditionIdParam = this.route.snapshot.paramMap.get('conditionId');

    this.form = this.fb.group({
      conditionId: [
        conditionIdParam ? Number(conditionIdParam) : '',
        [Validators.required],
      ],
      passed: [true, Validators.required],
      notes: [''],
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;

    const values = this.form.value;
    this.thresholdService
      .submitManualCheck(this.candidacyId, {
        candidacyId: this.candidacyId,
        conditionId: values.conditionId,
        passed: values.passed,
        notes: values.notes || undefined,
        userId: 0, // will be set by backend from auth context
      })
      .subscribe({
        next: () => {
          this.notification.success('בדיקה ידנית נשמרה בהצלחה');
          this.saving = false;
          this.onCancel();
        },
        error: () => {
          this.notification.error('שגיאה בשמירת בדיקה ידנית');
          this.saving = false;
        },
      });
  }

  onCancel(): void {
    this.router.navigate(['/threshold-checks/candidacy', this.candidacyId]);
  }
}
