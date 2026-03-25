import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { Contact } from '../../models/contact.models';
import { ContactService } from '../../services/contact.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsModalService } from '@igds/angular';
import { ConfirmDialogComponent } from '@shared/components/confirm-dialog/confirm-dialog.component';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-contact-list',
  template: `
    <div class="page-header">
      <h1>ניהול אנשי קשר</h1>
      <igds-button variant="primary" (onClick)="onCreate()">
        איש קשר חדש
      </igds-button>
    </div>

    <igds-card>
      <igds-search-field
        placeholder="חיפוש לפי שם, ת.ז. או דוא&quot;ל..."
        [value]="searchTerm"
        (search)="onSearch($event)"
        (clear)="onSearch('')">
      </igds-search-field>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && displayData.length === 0" class="no-data">
        לא נמצאו אנשי קשר
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
          <igds-button variant="secondary" [iconOnly]="true" [igdsTooltip]="'צפייה'" ariaLabel="צפייה" (onClick)="onView(row)">
            <span igds-icon>👁</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" [igdsTooltip]="'עריכה'" ariaLabel="עריכה" (onClick)="onEdit(row)">
            <span igds-icon>✏</span>
          </igds-button>
          <igds-button variant="secondary" [iconOnly]="true" [igdsTooltip]="'מחיקה'" ariaLabel="מחיקה" (onClick)="onDelete(row)">
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
export class ContactListComponent implements OnInit {
  columns: IgdsTableColumn[] = [
    { key: 'idNumber', label: 'ת.ז.', sortable: true },
    { key: 'firstName', label: 'שם פרטי', sortable: true },
    { key: 'lastName', label: 'שם משפחה', sortable: true },
    { key: 'phone', label: 'טלפון' },
    { key: 'email', label: 'דוא"ל' },
  ];

  allData: Contact[] = [];
  displayData: Contact[] = [];
  loading = false;
  searchTerm = '';

  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  sortColumn = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  private searchSubject = new Subject<string>();

  constructor(
    private contactService: ContactService,
    private router: Router,
    private modalService: IgdsModalService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadData();
    this.searchSubject
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((term: string) => {
        this.searchTerm = term;
        this.currentPage = 1;
        this.loadData(term);
      });
  }

  loadData(searchTerm?: string): void {
    this.loading = true;
    this.contactService.search(searchTerm).subscribe({
      next: (data: Contact[]) => {
        this.allData = data;
        this.totalItems = data.length;
        this.applySort();
        this.applyPagination();
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת אנשי קשר');
        this.loading = false;
      },
    });
  }

  onSearch(value: string): void {
    this.searchSubject.next(value.trim());
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

  onCreate(): void {
    this.router.navigate(['/contacts/new']);
  }

  onView(contact: Contact): void {
    this.router.navigate(['/contacts', contact.id]);
  }

  onEdit(contact: Contact): void {
    this.router.navigate(['/contacts', contact.id, 'edit']);
  }

  onDelete(contact: Contact): void {
    const modalRef = this.modalService.open<boolean>({
      title: 'מחיקת איש קשר',
      component: ConfirmDialogComponent,
      data: {
        title: 'מחיקת איש קשר',
        message: `האם למחוק את "${contact.firstName} ${contact.lastName}"?`,
        confirmText: 'מחיקה',
        cancelText: 'ביטול',
      },
    });

    modalRef.afterClosed().subscribe((confirmed: boolean | undefined) => {
      if (confirmed) {
        this.contactService.delete(contact.id).subscribe({
          next: () => {
            this.notification.success('איש הקשר נמחק בהצלחה');
            this.loadData(this.searchTerm || undefined);
          },
          error: () => this.notification.error('שגיאה במחיקת איש הקשר'),
        });
      }
    });
  }

  private applySort(): void {
    if (!this.sortColumn) return;
    const dir = this.sortDirection === 'asc' ? 1 : -1;
    this.allData = [...this.allData].sort((a: any, b: any) => {
      const valA = (a[this.sortColumn] || '').toString().toLowerCase();
      const valB = (b[this.sortColumn] || '').toString().toLowerCase();
      return valA.localeCompare(valB, 'he') * dir;
    });
  }

  private applyPagination(): void {
    const start = (this.currentPage - 1) * this.pageSize;
    this.displayData = this.allData.slice(start, start + this.pageSize);
  }
}
