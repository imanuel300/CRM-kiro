import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'igds-pagination',
  template: `
    <nav class="igds-pagination" role="navigation" aria-label="ניווט עמודים">
      <button class="igds-pagination__btn" [disabled]="currentPage <= 1"
        aria-label="עמוד קודם" (click)="goTo(currentPage - 1)">‹</button>
      <ng-container *ngFor="let p of pages">
        <span *ngIf="p === -1" class="igds-pagination__ellipsis">…</span>
        <button *ngIf="p !== -1" class="igds-pagination__btn"
          [class.igds-pagination__btn--active]="p === currentPage"
          [attr.aria-current]="p === currentPage ? 'page' : null"
          (click)="goTo(p)">{{p}}</button>
      </ng-container>
      <button class="igds-pagination__btn" [disabled]="currentPage >= totalPages"
        aria-label="עמוד הבא" (click)="goTo(currentPage + 1)">›</button>
    </nav>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-pagination {
      display: flex; align-items: center; gap: var(--igds-space-4);
      font-family: var(--igds-font-family); justify-content: center;
    }
    .igds-pagination__btn {
      min-width: 36px; height: 36px; display: flex; align-items: center; justify-content: center;
      border: 1px solid var(--igds-border-subtle-default); border-radius: var(--igds-radius-md);
      background: var(--igds-bg-neutral); cursor: pointer; font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm); color: var(--igds-text-primary);
      transition: all var(--igds-transition-fast);
    }
    .igds-pagination__btn:hover:not(:disabled) { border-color: var(--igds-border-subtle-hover); background: var(--igds-bg-neutral-hover); }
    .igds-pagination__btn--active {
      background: var(--igds-bg-brand-default); color: var(--igds-text-inverted);
      border-color: var(--igds-bg-brand-default);
    }
    .igds-pagination__btn:disabled { color: var(--igds-text-disabled); cursor: not-allowed; }
    .igds-pagination__btn:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    .igds-pagination__ellipsis { color: var(--igds-text-secondary); padding: 0 var(--igds-space-4); }
  `]
})
export class IgdsPaginationComponent {
  @Input() totalItems = 0;
  @Input() pageSize = 10;
  @Input() currentPage = 1;
  @Output() pageChange = new EventEmitter<number>();

  get totalPages(): number { return Math.ceil(this.totalItems / this.pageSize) || 1; }

  get pages(): number[] {
    const total = this.totalPages;
    if (total <= 7) return Array.from({ length: total }, (_, i) => i + 1);
    const pages: number[] = [1];
    if (this.currentPage > 3) pages.push(-1);
    for (let i = Math.max(2, this.currentPage - 1); i <= Math.min(total - 1, this.currentPage + 1); i++) pages.push(i);
    if (this.currentPage < total - 2) pages.push(-1);
    pages.push(total);
    return pages;
  }

  goTo(page: number) {
    if (page < 1 || page > this.totalPages || page === this.currentPage) return;
    this.currentPage = page;
    this.pageChange.emit(page);
  }
}
