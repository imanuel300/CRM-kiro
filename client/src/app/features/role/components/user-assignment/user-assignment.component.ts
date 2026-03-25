import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { Role, UserRole } from '../../models/role.models';
import { RoleApiService } from '../../services/role.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-user-assignment',
  template: `
    <div class="page-header">
      <h1>שיוך משתמשים לתפקיד: {{ role?.name }}</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div *ngIf="!loading">
      <igds-card class="assign-card">
        <h3 igds-card-header>הוספת שיוך חדש</h3>
        <form [formGroup]="assignForm" (ngSubmit)="onAssign()" class="assign-form">
          <igds-input-field
            label="מזהה משתמש"
            type="number"
            formControlName="userId"
            [required]="true"
            [error]="assignForm.get('userId')?.hasError('required') && assignForm.get('userId')?.touched ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="מזהה יחידה ארגונית"
            type="number"
            formControlName="orgUnitId"
            [required]="true"
            [error]="assignForm.get('orgUnitId')?.hasError('required') && assignForm.get('orgUnitId')?.touched ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-button variant="primary" type="submit"
                      [disabled]="assignForm.invalid || assigning">
            שיוך
          </igds-button>
        </form>
      </igds-card>

      <igds-card class="assignments-card">
        <h3 igds-card-header>משתמשים משויכים</h3>

        <div *ngIf="displayData.length === 0" class="no-data">
          אין משתמשים משויכים לתפקיד זה
        </div>

        <igds-table
          *ngIf="displayData.length > 0"
          [columns]="columns"
          [data]="displayData">
        </igds-table>

        <div *ngIf="displayData.length > 0" class="actions-column">
          <div *ngFor="let row of displayData" class="actions-row">
            <igds-button variant="secondary" [iconOnly]="true"
                        ariaLabel="הסרת שיוך"
                        [igdsTooltip]="'הסרת שיוך'"
                        (onClick)="onRemove(row)">
              <span igds-icon>🗑</span>
            </igds-button>
          </div>
        </div>

        <igds-pagination
          *ngIf="totalItems > 0"
          [totalItems]="totalItems"
          [pageSize]="pageSize"
          [currentPage]="currentPage"
          (pageChange)="onPageChange($event)">
        </igds-pagination>
      </igds-card>
    </div>
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
    .assign-card {
      margin-block-end: var(--igds-space-16);
    }
    .assign-form {
      display: flex;
      gap: var(--igds-space-12);
      align-items: flex-end;
      flex-wrap: wrap;
    }
    .assign-form > igds-input-field {
      flex: 1;
      min-width: 150px;
    }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .assignments-card {
      margin-block-start: var(--igds-space-16);
    }
    .actions-column { display: none; }
  `],
})
export class UserAssignmentComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'userId', label: 'מזהה משתמש' },
    { key: 'roleName', label: 'תפקיד' },
    { key: 'orgUnitId', label: 'יחידה ארגונית' },
    { key: 'assignedAtDisplay', label: 'תאריך שיוך' },
  ];

  allData: UserRole[] = [];
  displayData: any[] = [];
  loading = false;
  assigning = false;
  role?: Role;
  roleId!: number;
  assignForm!: FormGroup;

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private roleApi: RoleApiService,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.roleId = +this.route.snapshot.paramMap.get('id')!;

    this.assignForm = this.fb.group({
      userId: [null, Validators.required],
      orgUnitId: [1, Validators.required],
    });

    this.loadRole();
  }

  loadRole(): void {
    this.loading = true;
    this.roleApi.getById(this.roleId).subscribe({
      next: (role: Role) => {
        this.role = role;
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת התפקיד');
        this.loading = false;
      },
    });
  }

  onAssign(): void {
    if (this.assignForm.invalid) return;
    this.assigning = true;
    const value = this.assignForm.value;

    this.roleApi.assignUser({
      userId: value.userId,
      roleId: this.roleId,
      orgUnitId: value.orgUnitId,
    }).subscribe({
      next: (assignment: UserRole) => {
        this.notification.success('המשתמש שויך בהצלחה');
        this.allData = [...this.allData, assignment];
        this.totalItems = this.allData.length;
        this.applyPagination();
        this.assignForm.patchValue({ userId: null });
        this.assigning = false;
      },
      error: () => {
        this.notification.error('שגיאה בשיוך המשתמש');
        this.assigning = false;
      },
    });
  }

  onRemove(row: any): void {
    const assignment = this.allData.find(a => a.id === row.id);
    if (!assignment) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'הסרת שיוך',
      component: ConfirmDialogComponent,
      data: {
        title: 'הסרת שיוך',
        message: `האם להסיר את שיוך המשתמש ${assignment.userId} מתפקיד ${assignment.roleName}?`,
        confirmText: 'הסרה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.roleApi.removeAssignment(assignment.id).subscribe({
          next: () => {
            this.notification.success('השיוך הוסר בהצלחה');
            this.allData = this.allData.filter((a: UserRole) => a.id !== assignment.id);
            this.totalItems = this.allData.length;
            this.applyPagination();
          },
          error: () => this.notification.error('שגיאה בהסרת השיוך'),
        });
      }
    });
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyPagination();
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const paged = this.allData.slice(start, start + this.pageSize);
    this.displayData = paged.map(item => ({
      ...item,
      assignedAtDisplay: item.assignedAt,
    }));
  }
}
