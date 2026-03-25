import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { OrgUnitService } from '../../services/org-unit.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-org-unit-form',
  template: `
    <div class="page-header">
      <h1>{{ isEdit ? 'עריכת יחידה ארגונית' : 'יצירת יחידה ארגונית חדשה' }}</h1>
    </div>

    <igds-card>
      <div igds-card-body>
        <app-loading-spinner [loading]="loading"></app-loading-spinner>

        <form [formGroup]="form" (ngSubmit)="onSubmit()" *ngIf="!loading">
          <igds-input-field
            label="שם היחידה"
            formControlName="name"
            [required]="true"
            [error]="form.get('name')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="תיאור"
            formControlName="description">
          </igds-input-field>

          <igds-input-field
            label='דוא"ל'
            type="email"
            formControlName="contactEmail"
            [error]="form.get('contactEmail')?.hasError('email') ? 'כתובת דוא\"ל לא תקינה' : ''">
          </igds-input-field>

          <igds-input-field
            label="טלפון"
            formControlName="contactPhone">
          </igds-input-field>

          <div class="form-actions">
            <igds-button variant="primary" type="submit" [disabled]="form.invalid || saving">
              {{ saving ? 'שומר...' : (isEdit ? 'עדכון' : 'יצירה') }}
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
    .page-header { margin-block-end: var(--igds-space-16); }
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
export class OrgUnitFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  loading = false;
  saving = false;
  private orgUnitId?: number;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private orgUnitService: OrgUnitService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      contactEmail: ['', Validators.email],
      contactPhone: [''],
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.orgUnitId = +id;
      this.loadOrgUnit();
    }
  }

  private loadOrgUnit(): void {
    this.loading = true;
    this.orgUnitService.getById(this.orgUnitId!).subscribe({
      next: (unit) => {
        this.form.patchValue({
          name: unit.name,
          description: unit.description,
          contactEmail: unit.contactEmail,
          contactPhone: unit.contactPhone,
        });
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת היחידה');
        this.loading = false;
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;

    const request$ = this.isEdit
      ? this.orgUnitService.update({ id: this.orgUnitId!, ...value })
      : this.orgUnitService.create(value);

    request$.subscribe({
      next: () => {
        this.notification.success(this.isEdit ? 'היחידה עודכנה בהצלחה' : 'היחידה נוצרה בהצלחה');
        this.router.navigate(['/admin/org-units']);
      },
      error: () => {
        this.notification.error('שגיאה בשמירת היחידה');
        this.saving = false;
      },
    });
  }

  onCancel(): void {
    this.router.navigate(['/admin/org-units']);
  }
}
