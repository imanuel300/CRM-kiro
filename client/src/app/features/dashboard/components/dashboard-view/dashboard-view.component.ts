import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DashboardDataDto, AttentionItemDto, StageBreakdownDto } from '../../models/dashboard.models';
import { DashboardService } from '../../services/dashboard.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-dashboard-view',
  template: `
    <div class="page-header">
      <h1>לוח מחוונים</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div class="dashboard-grid" *ngIf="data && !loading">
      <!-- Active Candidacies Card -->
      <igds-card class="metric-card active-card">
        <div class="metric-content">
          <span class="metric-icon" aria-hidden="true">👥</span>
          <div class="metric-info">
            <span class="metric-value">{{ data.activeCandidacies }}</span>
            <span class="metric-label">מועמדויות פעילות</span>
          </div>
        </div>
      </igds-card>

      <!-- Attention Required Card -->
      <igds-card class="metric-card attention-card">
        <div class="metric-content">
          <span class="metric-icon" aria-hidden="true">⚠</span>
          <div class="metric-info">
            <span class="metric-value">{{ data.candidaciesRequiringAttention }}</span>
            <span class="metric-label">דורשות טיפול</span>
          </div>
        </div>
      </igds-card>

      <!-- Org Unit Name Card -->
      <igds-card class="metric-card unit-card">
        <div class="metric-content">
          <span class="metric-icon" aria-hidden="true">🏢</span>
          <div class="metric-info">
            <span class="metric-value unit-name">{{ data.orgUnitName }}</span>
            <span class="metric-label">יחידה ארגונית</span>
          </div>
        </div>
      </igds-card>
    </div>

    <!-- Screening Stage Breakdown -->
    <igds-card class="chart-card" *ngIf="data && !loading && data.byScreeningStage.length">
      <h2 igds-card-header class="card-title">פילוח לפי שלב מיון</h2>
      <div class="bar-chart">
        <div class="bar-row" *ngFor="let stage of data.byScreeningStage">
          <span class="bar-label">{{ stage.stageDisplayName }}</span>
          <div class="bar-track">
            <div class="bar-fill"
                 [style.width.%]="getBarWidth(stage.count)"
                 igdsTooltip="{{ stage.count }} מועמדויות">
            </div>
          </div>
          <span class="bar-value">{{ stage.count }}</span>
        </div>
      </div>
    </igds-card>

    <!-- Attention Items -->
    <igds-card class="attention-list-card" *ngIf="data && !loading && data.attentionItems.length">
      <h2 igds-card-header class="card-title">מועמדויות הדורשות טיפול</h2>

      <igds-table
        [columns]="attentionColumns"
        [data]="attentionTableData">
      </igds-table>

      <div class="attention-actions">
        <div *ngFor="let item of data.attentionItems" class="attention-action-row">
          <igds-tag [label]="item.statusDisplayName" variant="warning"></igds-tag>
          <igds-button variant="secondary" [iconOnly]="true"
            ariaLabel="צפה במועמדות"
            igdsTooltip="צפה במועמדות"
            (onClick)="goToCandidacy(item.candidacyId)">
            <span igds-icon>👁</span>
          </igds-button>
        </div>
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
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }

    .dashboard-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: var(--igds-space-16);
      margin-block-end: var(--igds-space-16);
    }

    .metric-content {
      display: flex;
      align-items: center;
      gap: var(--igds-space-16);
      padding: var(--igds-space-8) 0;
    }
    .metric-icon {
      font-size: 40px;
      width: 40px;
      height: 40px;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .active-card .metric-icon { color: var(--igds-text-link-default); }
    .attention-card .metric-icon { color: var(--igds-text-warning); }
    .unit-card .metric-icon { color: var(--igds-text-success); }
    .metric-info {
      display: flex;
      flex-direction: column;
    }
    .metric-value {
      font-size: 28px;
      font-weight: var(--igds-font-weight-bold, 600);
      line-height: 1.2;
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .metric-value.unit-name { font-size: var(--igds-font-size-lg); }
    .metric-label {
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }

    .card-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-lg);
      font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary);
      margin: 0;
    }

    .chart-card, .attention-list-card {
      margin-block-end: var(--igds-space-16);
    }

    .bar-chart { padding: var(--igds-space-8) 0; }
    .bar-row {
      display: flex;
      align-items: center;
      gap: var(--igds-space-12);
      margin-block-end: var(--igds-space-8);
    }
    .bar-label {
      min-width: 140px;
      text-align: start;
      font-size: var(--igds-font-size-sm);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .bar-track {
      flex: 1;
      height: 24px;
      background: var(--igds-bg-neutral-secondlevel);
      border-radius: var(--igds-radius-sm);
      overflow: hidden;
    }
    .bar-fill {
      height: 100%;
      background: var(--igds-bg-brand-default);
      border-radius: var(--igds-radius-sm);
      min-width: 2px;
      transition: width var(--igds-transition-fast);
    }
    .bar-value {
      min-width: 40px;
      text-align: center;
      font-weight: var(--igds-font-weight-medium);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }

    .attention-actions { display: none; }
  `],
})
export class DashboardViewComponent implements OnInit {
  data: DashboardDataDto | null = null;
  loading = false;
  maxStageCount = 1;

  attentionColumns: IgdsTableColumn[] = [
    { key: 'candidacyId', label: 'מזהה' },
    { key: 'statusDisplayName', label: 'סטטוס' },
    { key: 'reason', label: 'סיבה' },
    { key: 'lastUpdated', label: 'עדכון אחרון' },
  ];

  attentionTableData: any[] = [];

  constructor(
    private dashboardService: DashboardService,
    private notification: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadDashboard();
  }

  loadDashboard(): void {
    this.loading = true;
    const orgUnitId = 1;
    this.dashboardService.getDashboardData(orgUnitId).subscribe({
      next: (data: DashboardDataDto) => {
        this.data = data;
        this.maxStageCount = Math.max(1, ...data.byScreeningStage.map((s: StageBreakdownDto) => s.count));
        this.attentionTableData = data.attentionItems.map((item: AttentionItemDto) => ({
          candidacyId: item.candidacyId,
          statusDisplayName: item.statusDisplayName,
          reason: item.reason,
          lastUpdated: item.lastUpdated,
        }));
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת לוח מחוונים');
        this.loading = false;
      },
    });
  }

  getBarWidth(count: number): number {
    return (count / this.maxStageCount) * 100;
  }

  goToCandidacy(candidacyId: number): void {
    this.router.navigate(['/candidacies', candidacyId]);
  }
}
