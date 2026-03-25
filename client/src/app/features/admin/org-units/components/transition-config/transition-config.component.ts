import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OrgUnitService } from '../../services/org-unit.service';
import { NotificationService } from '@core/services/notification.service';
import {
  StatusDefinition,
  StatusTransition,
  ConfigureTransitionDefinition,
} from '../../models/org-unit.models';

interface TransitionCell {
  fromCode: string;
  toCode: string;
  allowed: boolean;
  requiresReason: boolean;
}

@Component({
  selector: 'app-transition-config',
  template: `
    <div class="page-header">
      <h1>הגדרת מעברי סטטוס</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <igds-card *ngIf="!loading">
      <div igds-card-header>
        <h2 class="card-title">מטריצת מעברים</h2>
        <p class="card-subtitle">סמן מעברים מותרים בין סטטוסים</p>
      </div>

      <div class="matrix-container" *ngIf="statuses.length > 0">
        <table class="transition-matrix">
          <thead>
            <tr>
              <th class="corner-cell">מ ↓ / אל →</th>
              <th *ngFor="let s of statuses" class="header-cell">
                {{ s.displayName }}
              </th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let from of statuses">
              <td class="row-header">{{ from.displayName }}</td>
              <td *ngFor="let to of statuses" class="matrix-cell"
                  [class.same-status]="from.code === to.code"
                  [class.active]="isAllowed(from.code, to.code)">
                <igds-checkbox
                  *ngIf="from.code !== to.code"
                  [checked]="isAllowed(from.code, to.code)"
                  (change)="toggleTransition(from.code, to.code, !isAllowed(from.code, to.code))">
                </igds-checkbox>
                <span *ngIf="from.code === to.code" class="same-marker">—</span>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <div *ngIf="statuses.length === 0" class="no-statuses">
        <span class="no-statuses-icon">ℹ️</span>
        <span>יש להגדיר סטטוסים תחילה</span>
        <igds-button variant="secondary" [routerLink]="['/admin/org-units', orgUnitId, 'statuses']">
          הגדרת סטטוסים
        </igds-button>
      </div>

      <div class="form-actions" *ngIf="statuses.length > 0">
        <igds-button variant="primary" (onClick)="onSave()" [disabled]="saving">
          {{ saving ? 'שומר...' : 'שמירה' }}
        </igds-button>
        <igds-button variant="secondary" routerLink="/admin/org-units">
          חזרה לרשימה
        </igds-button>
      </div>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header { margin-block-end: var(--igds-space-16); }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .card-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-lg);
      font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .card-subtitle {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-secondary);
      margin: var(--igds-space-4) 0 0;
    }
    .matrix-container { overflow-x: auto; margin: var(--igds-space-16) 0; }
    .transition-matrix {
      border-collapse: collapse;
      width: 100%;
      min-width: 400px;
      font-family: var(--igds-font-family);
    }
    .transition-matrix th,
    .transition-matrix td {
      border: 1px solid var(--igds-border-divider);
      padding: var(--igds-space-8);
      text-align: center;
      min-width: 80px;
    }
    .corner-cell {
      background: var(--igds-bg-neutral-secondlevel);
      font-weight: var(--igds-font-weight-medium);
      text-align: start;
      white-space: nowrap;
      color: var(--igds-text-primary);
    }
    .header-cell {
      background: var(--igds-bg-neutral-secondlevel);
      font-weight: var(--igds-font-weight-medium);
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-primary);
    }
    .row-header {
      background: var(--igds-bg-neutral-secondlevel);
      font-weight: var(--igds-font-weight-medium);
      text-align: start;
      font-size: var(--igds-font-size-sm);
      white-space: nowrap;
      color: var(--igds-text-primary);
    }
    .matrix-cell { background: var(--igds-bg-neutral); }
    .matrix-cell.same-status { background: var(--igds-bg-neutral-secondlevel); }
    .matrix-cell.active { background: var(--igds-bg-success-subtle); }
    .same-marker { color: var(--igds-text-disabled); }
    .no-statuses {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
      justify-content: center;
    }
    .no-statuses-icon { font-size: var(--igds-font-size-lg); }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
  `],
})
export class TransitionConfigComponent implements OnInit {
  loading = false;
  saving = false;
  orgUnitId!: number;
  statuses: StatusDefinition[] = [];
  transitionMap = new Map<string, TransitionCell>();

  constructor(
    private route: ActivatedRoute,
    private orgUnitService: OrgUnitService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.orgUnitId = +this.route.snapshot.paramMap.get('id')!;
    this.loadData();
  }

  private loadData(): void {
    this.loading = true;
    this.orgUnitService.getStatuses(this.orgUnitId).subscribe({
      next: (statuses: StatusDefinition[]) => {
        this.statuses = statuses;
        this.orgUnitService.getTransitions(this.orgUnitId).subscribe({
          next: (transitions: StatusTransition[]) => {
            this.buildTransitionMap(transitions);
            this.loading = false;
          },
          error: () => (this.loading = false),
        });
      },
      error: () => (this.loading = false),
    });
  }

  private buildTransitionMap(transitions: StatusTransition[]): void {
    this.transitionMap.clear();
    transitions.forEach((t) => {
      const key = `${t.fromStatusCode}|${t.toStatusCode}`;
      this.transitionMap.set(key, {
        fromCode: t.fromStatusCode,
        toCode: t.toStatusCode,
        allowed: true,
        requiresReason: t.requiresReason,
      });
    });
  }

  isAllowed(fromCode: string, toCode: string): boolean {
    return this.transitionMap.has(`${fromCode}|${toCode}`);
  }

  toggleTransition(fromCode: string, toCode: string, allowed: boolean): void {
    const key = `${fromCode}|${toCode}`;
    if (allowed) {
      this.transitionMap.set(key, { fromCode, toCode, allowed: true, requiresReason: false });
    } else {
      this.transitionMap.delete(key);
    }
  }

  onSave(): void {
    this.saving = true;
    const transitions: ConfigureTransitionDefinition[] = Array.from(
      this.transitionMap.values()
    ).map((t) => ({
      fromStatusCode: t.fromCode,
      toStatusCode: t.toCode,
      requiresReason: t.requiresReason,
    }));

    this.orgUnitService
      .configureTransitions({ orgUnitId: this.orgUnitId, transitions })
      .subscribe({
        next: () => {
          this.notification.success('מעברי הסטטוס עודכנו בהצלחה');
          this.saving = false;
        },
        error: () => {
          this.notification.error('שגיאה בשמירת מעברי הסטטוס');
          this.saving = false;
        },
      });
  }
}
