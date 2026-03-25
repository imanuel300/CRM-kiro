import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { ConflictService } from '../../services/conflict.service';
import { NotificationService } from '@core/services/notification.service';
import { ConflictOfInterest, FamilyRelation, RELATION_TYPES } from '../../models/conflict.models';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-questionnaire-form',
  template: `
    <div class="page-header">
      <h1>{{ isFamily ? 'הצהרת קרבה משפחתית' : 'הצהרת ניגוד עניינים' }}</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <!-- טופס ניגוד עניינים -->
    <igds-card *ngIf="!loading && !isFamily">
      <form [formGroup]="conflictForm" (ngSubmit)="onSubmitConflict()">
        <div class="igds-textarea-field">
          <label class="igds-textarea-field__label" for="questionnaireResponses">
            תשובות שאלון ניגוד עניינים <span class="igds-textarea-field__required">*</span>
          </label>
          <textarea
            id="questionnaireResponses"
            class="igds-textarea-field__input"
            [class.igds-textarea-field__input--error]="conflictForm.get('questionnaireResponses')?.hasError('required') && conflictForm.get('questionnaireResponses')?.touched"
            formControlName="questionnaireResponses"
            rows="6"
            placeholder="נא למלא את תשובות השאלון"
            [attr.aria-invalid]="conflictForm.get('questionnaireResponses')?.hasError('required') && conflictForm.get('questionnaireResponses')?.touched"
            [attr.aria-describedby]="conflictForm.get('questionnaireResponses')?.hasError('required') && conflictForm.get('questionnaireResponses')?.touched ? 'questionnaireResponses-error' : null">
          </textarea>
          <span *ngIf="conflictForm.get('questionnaireResponses')?.hasError('required') && conflictForm.get('questionnaireResponses')?.touched"
                id="questionnaireResponses-error"
                class="igds-textarea-field__error" role="alert">
            שדה חובה
          </span>
        </div>

        <igds-checkbox
          formControlName="hasConflict"
          label="קיים ניגוד עניינים"
          class="conflict-checkbox">
        </igds-checkbox>

        <div class="form-actions">
          <igds-button variant="primary" type="submit"
                       [disabled]="conflictForm.invalid || saving">
            {{ isEdit ? 'עדכון' : 'שמירה' }}
          </igds-button>
          <igds-button variant="secondary" type="button" (onClick)="onCancel()">ביטול</igds-button>
        </div>
      </form>
    </igds-card>

    <!-- טופס קרבה משפחתית -->
    <igds-card *ngIf="!loading && isFamily">
      <form [formGroup]="familyForm" (ngSubmit)="onSubmitFamily()">
        <igds-dropdown
          label="סוג קרבה"
          formControlName="relationType"
          [options]="relationTypeOptions"
          [required]="true"
          [error]="familyForm.get('relationType')?.hasError('required') && familyForm.get('relationType')?.touched ? 'שדה חובה' : ''">
        </igds-dropdown>

        <igds-input-field
          label="שם הקרוב"
          formControlName="relatedPersonName"
          [required]="true"
          [error]="familyForm.get('relatedPersonName')?.hasError('required') && familyForm.get('relatedPersonName')?.touched ? 'שדה חובה' : ''">
        </igds-input-field>

        <igds-input-field
          label="תפקיד הקרוב (אופציונלי)"
          formControlName="relatedPersonRole">
        </igds-input-field>

        <div class="form-actions">
          <igds-button variant="primary" type="submit"
                       [disabled]="familyForm.invalid || saving">
            {{ isEdit ? 'עדכון' : 'שמירה' }}
          </igds-button>
          <igds-button variant="secondary" type="button" (onClick)="onCancel()">ביטול</igds-button>
        </div>
      </form>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header { margin-block-end: var(--igds-space-16); }
    .page-header h1 { font-family: var(--igds-font-family); color: var(--igds-text-primary); }
    .conflict-checkbox { display: block; margin-block: var(--igds-space-16); }
    .form-actions { display: flex; gap: var(--igds-space-8); margin-block-start: var(--igds-space-16); }

    igds-dropdown,
    igds-input-field,
    .igds-textarea-field { margin-block-end: var(--igds-space-16); }

    .igds-textarea-field__label {
      display: block; font-family: var(--igds-font-family); font-size: var(--igds-font-size-sm);
      font-weight: var(--igds-font-weight-medium); color: var(--igds-text-primary);
      margin-block-end: var(--igds-space-4);
    }
    .igds-textarea-field__required { color: var(--igds-text-failure); }
    .igds-textarea-field__input {
      width: 100%; padding: var(--igds-space-8) var(--igds-space-12);
      border: 1px solid var(--igds-border-subtle-default); border-radius: var(--igds-radius-md);
      font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary); background: var(--igds-bg-neutral);
      direction: inherit; resize: vertical; box-sizing: border-box;
      transition: border-color var(--igds-transition-fast);
    }
    .igds-textarea-field__input::placeholder { color: var(--igds-text-secondary); }
    .igds-textarea-field__input:hover { border-color: var(--igds-border-subtle-hover); }
    .igds-textarea-field__input:focus { border-color: var(--igds-border-active); border-width: 2px; outline: none; }
    .igds-textarea-field__input:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    .igds-textarea-field__input--error { border-color: var(--igds-border-failure); }
    .igds-textarea-field__error {
      display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-failure);
      margin-block-start: var(--igds-space-4);
    }
  `],
})
export class QuestionnaireFormComponent implements OnInit {
  isFamily = false;
  isEdit = false;
  loading = false;
  saving = false;
  editId = 0;
  candidacyId = 0;
  contactId = 0;
  relationTypes = RELATION_TYPES;
  relationTypeOptions: IgdsDropdownOption[] = RELATION_TYPES.map(t => ({ value: t.value, label: t.label }));

  conflictForm!: FormGroup;
  familyForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private conflictService: ConflictService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.isFamily = this.route.snapshot.queryParamMap.get('type') === 'family';
    this.candidacyId = Number(
      this.route.snapshot.paramMap.get('candidacyId') ||
      this.route.snapshot.queryParamMap.get('candidacyId') ||
      0
    );

    const idParam = this.route.snapshot.paramMap.get('id');
    this.isEdit = idParam !== 'new' && idParam != null;
    this.editId = this.isEdit ? Number(idParam) : 0;

    this.conflictForm = this.fb.group({
      questionnaireResponses: ['', Validators.required],
      hasConflict: [false],
    });

    this.familyForm = this.fb.group({
      relationType: ['', Validators.required],
      relatedPersonName: ['', Validators.required],
      relatedPersonRole: [''],
    });

    if (this.isEdit) {
      this.loadExisting();
    }
  }

  loadExisting(): void {
    this.loading = true;
    if (this.isFamily) {
      this.conflictService.getFamilyRelation(this.editId).subscribe({
        next: (data: FamilyRelation) => {
          this.candidacyId = data.candidacyId;
          this.contactId = data.contactId;
          this.familyForm.patchValue({
            relationType: data.relationType,
            relatedPersonName: data.relatedPersonName,
            relatedPersonRole: data.relatedPersonRole,
          });
          this.loading = false;
        },
        error: () => {
          this.notification.error('שגיאה בטעינת הצהרה');
          this.loading = false;
        },
      });
    } else {
      this.conflictService.getConflict(this.editId).subscribe({
        next: (data: ConflictOfInterest) => {
          this.candidacyId = data.candidacyId;
          this.contactId = data.contactId;
          this.conflictForm.patchValue({
            questionnaireResponses: data.questionnaireResponses,
            hasConflict: data.hasConflict,
          });
          this.loading = false;
        },
        error: () => {
          this.notification.error('שגיאה בטעינת הצהרה');
          this.loading = false;
        },
      });
    }
  }

  onSubmitConflict(): void {
    if (this.conflictForm.invalid) return;
    this.saving = true;
    const values = this.conflictForm.value;

    if (this.isEdit) {
      this.conflictService.updateConflict({
        id: this.editId,
        questionnaireResponses: values.questionnaireResponses,
        hasConflict: values.hasConflict,
      }).subscribe({
        next: () => {
          this.notification.success('ההצהרה עודכנה בהצלחה');
          this.saving = false;
          this.onCancel();
        },
        error: () => {
          this.notification.error('שגיאה בעדכון ההצהרה');
          this.saving = false;
        },
      });
    } else {
      this.conflictService.createConflict({
        candidacyId: this.candidacyId,
        contactId: this.contactId,
        questionnaireResponses: values.questionnaireResponses,
        hasConflict: values.hasConflict,
      }).subscribe({
        next: () => {
          this.notification.success('ההצהרה נוצרה בהצלחה');
          this.saving = false;
          this.onCancel();
        },
        error: () => {
          this.notification.error('שגיאה ביצירת ההצהרה');
          this.saving = false;
        },
      });
    }
  }

  onSubmitFamily(): void {
    if (this.familyForm.invalid) return;
    this.saving = true;
    const values = this.familyForm.value;

    if (this.isEdit) {
      this.conflictService.updateFamilyRelation({
        id: this.editId,
        relationType: values.relationType,
        relatedPersonName: values.relatedPersonName,
        relatedPersonRole: values.relatedPersonRole || undefined,
      }).subscribe({
        next: () => {
          this.notification.success('ההצהרה עודכנה בהצלחה');
          this.saving = false;
          this.onCancel();
        },
        error: () => {
          this.notification.error('שגיאה בעדכון ההצהרה');
          this.saving = false;
        },
      });
    } else {
      this.conflictService.createFamilyRelation({
        candidacyId: this.candidacyId,
        contactId: this.contactId,
        relationType: values.relationType,
        relatedPersonName: values.relatedPersonName,
        relatedPersonRole: values.relatedPersonRole || undefined,
      }).subscribe({
        next: () => {
          this.notification.success('ההצהרה נוצרה בהצלחה');
          this.saving = false;
          this.onCancel();
        },
        error: () => {
          this.notification.error('שגיאה ביצירת ההצהרה');
          this.saving = false;
        },
      });
    }
  }

  onCancel(): void {
    if (this.candidacyId) {
      this.router.navigate(['/conflicts/candidacy', this.candidacyId]);
    } else {
      this.router.navigate(['/conflicts']);
    }
  }
}
