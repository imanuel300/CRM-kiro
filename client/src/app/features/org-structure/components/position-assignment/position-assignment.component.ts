import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { OrgStructureApiService } from '../../services/org-structure.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-position-assignment',
  template: `
    <div class="page-header">
      <h1>שיוך מועמד לתפקיד</h1>
    </div>

    <igds-card>
      <div igds-card-body>
        <form [formGroup]="assignForm" (ngSubmit)="onAssign()">
          <igds-input-field
            label="מזהה תפקיד"
            type="number"
            formControlName="orgPositionId"
            [required]="true"
            [error]="assignForm.get('orgPositionId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="מזהה איש קשר"
            type="number"
            formControlName="contactId"
            [required]="true"
            [error]="assignForm.get('contactId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="מזהה מועמדות"
            type="number"
            formControlName="candidacyId"
            [required]="true"
            [error]="assignForm.get('candidacyId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-date-picker
            label="תאריך התחלה"
            formControlName="startDate"
            [required]="true"
            [error]="assignForm.get('startDate')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-date-picker>

          <div class="form-actions">
            <igds-button variant="primary" type="submit"
                         [disabled]="assignForm.invalid || saving">
              שיוך לתפקיד
            </igds-button>
          </div>
        </form>

        <hr class="divider" />

        <h2 class="section-title">הסרת שיוך</h2>
        <form [formGroup]="unassignForm" (ngSubmit)="onUnassign()">
          <igds-input-field
            label="מזהה שיוך"
            type="number"
            formControlName="assignmentId"
            [required]="true"
            [error]="unassignForm.get('assignmentId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <div class="form-actions">
            <igds-button variant="primary" type="submit"
                         [disabled]="unassignForm.invalid || saving">
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
    .section-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-lg);
      color: var(--igds-text-primary);
    }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
    .divider {
      margin-block: var(--igds-space-24);
      border: none;
      border-block-start: 1px solid var(--igds-border-divider);
    }
  `],
})
export class PositionAssignmentComponent {
  assignForm: FormGroup;
  unassignForm: FormGroup;
  saving = false;

  constructor(
    private fb: FormBuilder,
    private orgStructureApi: OrgStructureApiService,
    private notification: NotificationService
  ) {
    this.assignForm = this.fb.group({
      orgPositionId: [null, Validators.required],
      contactId: [null, Validators.required],
      candidacyId: [null, Validators.required],
      startDate: [null, Validators.required],
    });

    this.unassignForm = this.fb.group({
      assignmentId: [null, Validators.required],
    });
  }

  onAssign(): void {
    if (this.assignForm.invalid) return;
    this.saving = true;
    const value = this.assignForm.value;

    this.orgStructureApi.assignToPosition({
      orgPositionId: value.orgPositionId,
      contactId: value.contactId,
      candidacyId: value.candidacyId,
      startDate: new Date(value.startDate).toISOString(),
    }).subscribe({
      next: () => {
        this.notification.success('המועמד שויך לתפקיד בהצלחה');
        this.saving = false;
        this.assignForm.reset();
      },
      error: () => {
        this.notification.error('שגיאה בשיוך מועמד לתפקיד');
        this.saving = false;
      },
    });
  }

  onUnassign(): void {
    if (this.unassignForm.invalid) return;
    this.saving = true;
    const { assignmentId } = this.unassignForm.value;

    this.orgStructureApi.unassignFromPosition({ assignmentId }).subscribe({
      next: () => {
        this.notification.success('השיוך הוסר בהצלחה');
        this.saving = false;
        this.unassignForm.reset();
      },
      error: () => {
        this.notification.error('שגיאה בהסרת שיוך');
        this.saving = false;
      },
    });
  }
}
