import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { Role, PermissionType, PERMISSION_LABELS } from '../../models/role.models';
import { RoleApiService } from '../../services/role.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-role-form',
  template: `
    <div class="page-header">
      <h1>{{ isEdit ? 'עריכת תפקיד' : 'תפקיד חדש' }}</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <igds-card *ngIf="!loading">
      <form [formGroup]="form" (ngSubmit)="onSubmit()">
        <igds-input-field
          label="שם התפקיד"
          formControlName="name"
          [required]="true"
          [error]="form.get('name')?.hasError('required') && form.get('name')?.touched ? 'שדה חובה' : ''">
        </igds-input-field>

        <div class="field-spacer">
          <igds-input-field
            label="תיאור"
            formControlName="description">
          </igds-input-field>
        </div>

        <div class="field-spacer">
          <igds-checkbox
            formControlName="allowCrossUnit"
            label="גישה חוצת יחידות ארגוניות">
          </igds-checkbox>
        </div>

        <div class="permissions-section">
          <h3>הרשאות</h3>
          <div class="permissions-grid">
            <igds-checkbox
              *ngFor="let perm of permissionOptions"
              [label]="perm.label"
              [checked]="isPermissionSelected(perm.value)"
              (change)="togglePermission(perm.value, $event)">
            </igds-checkbox>
          </div>
        </div>

        <div class="form-actions">
          <igds-button variant="primary" type="submit"
                      [disabled]="form.invalid || saving">
            {{ isEdit ? 'עדכון' : 'יצירה' }}
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
    .field-spacer {
      margin-block-start: var(--igds-space-16);
    }
    .permissions-section {
      margin-block-start: var(--igds-space-16);
      margin-block-end: var(--igds-space-16);
    }
    .permissions-section h3 {
      margin-block-end: var(--igds-space-8);
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-secondary);
    }
    .permissions-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
      gap: var(--igds-space-8);
    }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
  `],
})
export class RoleFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  loading = false;
  saving = false;
  roleId?: number;
  selectedPermissions: PermissionType[] = [];
  permissionOptions = PERMISSION_LABELS;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private roleApi: RoleApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', Validators.required],
      description: [''],
      allowCrossUnit: [false],
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit = true;
      this.roleId = +id;
      this.loadRole(this.roleId);
    }
  }

  loadRole(id: number): void {
    this.loading = true;
    this.roleApi.getById(id).subscribe({
      next: (role: Role) => {
        this.form.patchValue({
          name: role.name,
          description: role.description,
          allowCrossUnit: role.allowCrossUnit,
        });
        this.selectedPermissions = [...role.permissions];
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת התפקיד');
        this.loading = false;
      },
    });
  }

  isPermissionSelected(permission: PermissionType): boolean {
    return this.selectedPermissions.includes(permission);
  }

  togglePermission(permission: PermissionType, event: Event): void {
    const checked = (event.target as HTMLInputElement)?.checked ??
      !(this.selectedPermissions.includes(permission));
    if (checked) {
      if (!this.selectedPermissions.includes(permission)) {
        this.selectedPermissions.push(permission);
      }
    } else {
      this.selectedPermissions = this.selectedPermissions.filter(p => p !== permission);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;

    if (this.isEdit && this.roleId) {
      this.roleApi.update({
        id: this.roleId,
        name: value.name,
        description: value.description,
        allowCrossUnit: value.allowCrossUnit,
        permissions: this.selectedPermissions,
      }).subscribe({
        next: () => {
          this.notification.success('התפקיד עודכן בהצלחה');
          this.router.navigate(['/roles']);
        },
        error: () => {
          this.notification.error('שגיאה בעדכון התפקיד');
          this.saving = false;
        },
      });
    } else {
      this.roleApi.create({
        name: value.name,
        description: value.description,
        orgUnitId: 1, // Default; in real app comes from auth context
        allowCrossUnit: value.allowCrossUnit,
        permissions: this.selectedPermissions,
      }).subscribe({
        next: () => {
          this.notification.success('התפקיד נוצר בהצלחה');
          this.router.navigate(['/roles']);
        },
        error: () => {
          this.notification.error('שגיאה ביצירת התפקיד');
          this.saving = false;
        },
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/roles']);
  }
}
