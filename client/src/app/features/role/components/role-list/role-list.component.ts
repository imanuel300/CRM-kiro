import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Role, PERMISSION_LABELS } from '../../models/role.models';
import { RoleApiService } from '../../services/role.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-role-list',
  template: `
    <div class="page-header">
      <h1>ניהול תפקידים והרשאות</h1>
      <igds-button variant="primary" (onClick)="onCreate()"
              *appHasPermission="'Create'">
        תפקיד חדש
      </igds-button>
    </div>

    <igds-card>
      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו תפקידים
      </div>

      <igds-table
        *ngIf="!loading && displayData.length > 0"
        [columns]="columns"
        [data]="displayData"
        [sortColumn]="sortColumn"
        [sortDirection]="sortDirection"
        (sort)="onSort($event)">
      </igds-table>

      <div *ngIf="!loading && displayData.length > 0" class="actions-column">
        <div *ngFor="let row of displayData" class="actions-row">
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="עריכה" (onClick)="onEdit(row)">
            <span igds-icon>✏️</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="שיוך משתמשים" (onClick)="onAssignUsers(row)">
            <span igds-icon>👤</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" ariaLabel="מחיקה" (onClick)="onDelete(row)"
                  *appHasPermission="'Delete'">
            <span igds-icon>🗑</span>
          </igds-button>
        </div>
      </div>

      <igds-pagination
        *ngIf="!loading && totalItems > 0"
        [totalItems]="totalItems"
        [pageSize]="pageSize"
        [currentPage]="currentPage"
        (pageChange)="onPageChange($event)">
      </igds-pagination>
    </igds-card>
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
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .actions-column { display: none; }
  `],
})
export class RoleListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'name', label: 'שם התפקיד', sortable: true },
    { key: 'description', label: 'תיאור' },
    { key: 'permissionsDisplay', label: 'הרשאות' },
    { key: 'crossUnitDisplay', label: 'חוצה יחידות' },
  ];

  allData: Role[] = [];
  displayData: any[] = [];
  loading = false;

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  constructor(
    private roleApi: RoleApiService,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadData();
  }

  onSort(event: { column: string; direction: 'asc' | 'desc' }): void {
    this.sortColumn = event.column;
    this.sortDirection = event.direction;
    this.applySort();
    this.applyPagination();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.applyPagination();
  }

  loadData(): void {
    this.loading = true;
    this.roleApi.listByOrgUnit(1).subscribe({
      next: (data: Role[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת תפקידים');
        this.loading = false;
      },
    });
  }

  getPermissionLabel(permission: string): string {
    return PERMISSION_LABELS.find(p => p.value === permission)?.label ?? permission;
  }

  onCreate(): void {
    this.router.navigate(['/roles/new']);
  }

  onEdit(row: any): void {
    const role = this.allData.find(r => r.id === row.id);
    if (role) {
      this.router.navigate(['/roles', role.id]);
    }
  }

  onAssignUsers(row: any): void {
    const role = this.allData.find(r => r.id === row.id);
    if (role) {
      this.router.navigate(['/roles', role.id, 'assign']);
    }
  }

  onDelete(row: any): void {
    const role = this.allData.find(r => r.id === row.id);
    if (!role) return;

    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת תפקיד',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת תפקיד',
        message: `האם למחוק את התפקיד "${role.name}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.roleApi.delete(role.id).subscribe({
          next: () => {
            this.notification.success('התפקיד נמחק בהצלחה');
            this.loadData();
          },
          error: () => this.notification.error('שגיאה במחיקת התפקיד'),
        });
      }
    });
  }

  private applySort(): void {
    if (!this.sortColumn) return;
    const dir = this.sortDirection === 'asc' ? 1 : -1;
    this.allData = [...this.allData].sort((a: any, b: any) => {
      const valA = (a[this.sortColumn] ?? '').toString().toLowerCase();
      const valB = (b[this.sortColumn] ?? '').toString().toLowerCase();
      return valA.localeCompare(valB, 'he') * dir;
    });
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    const paged = this.allData.slice(start, start + this.pageSize);
    this.displayData = paged.map(item => ({
      ...item,
      description: item.description || '—',
      permissionsDisplay: item.permissions.map(p => this.getPermissionLabel(p)).join(', '),
      crossUnitDisplay: item.allowCrossUnit ? '✓' : '✗',
    }));
  }
}
