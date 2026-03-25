import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ThresholdCheckService } from '../../services/threshold-check.service';
import { NotificationService } from '@core/services/notification.service';
import {
  ThresholdCheckResult,
  ThresholdCheckSummary,
  CONDITION_TYPE_LABELS,
} from '../../models/threshold-check.models';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-threshold-results',
  template: `
    <div class="page-header">
      <h1>תוצאות בדיקת תנאי סף</h1>
      <span class="spacer"></span>
      <igds-button
        variant="primary"
        [disabled]="evaluating"
        (onClick)="onEvaluateAll()">
        ▶ הרצת בדיקה אוטומטית
      </igds-button>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <igds-card *ngIf="!loading && summary" class="summary-card">
      <div class="summary-row">
        <igds-status-badge
          [variant]="summary.allPassed ? 'success' : 'failure'"
          [text]="summary.allPassed ? 'עבר את כל תנאי הסף' : 'לא עמד בכל תנאי הסף'">
        </igds-status-badge>
      </div>
    </igds-card>

    <igds-table
      *ngIf="!loading && results.length > 0"
      [columns]="tableColumns"
      [data]="tableData">
    </igds-table>

    <igds-card *ngIf="!loading && results.length === 0" class="empty-card">
      <p class="empty-text">לא נמצאו תוצאות בדיקת תנאי סף למועמדות זו</p>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .spacer { flex: 1; }
    .summary-card { margin-block-end: var(--igds-space-16); }
    .summary-row {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      font-size: var(--igds-font-size-lg);
    }
    .empty-text {
      text-align: center;
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
  `],
})
export class ThresholdResultsComponent implements OnInit {
  candidacyId!: number;
  results: ThresholdCheckResult[] = [];
  summary: ThresholdCheckSummary | null = null;
  loading = false;
  evaluating = false;

  tableColumns: IgdsTableColumn[] = [
    { key: 'conditionType', label: 'סוג תנאי' },
    { key: 'fieldName', label: 'שדה' },
    { key: 'passed', label: 'תוצאה' },
    { key: 'actualValue', label: 'ערך בפועל' },
    { key: 'isAutomatic', label: 'סוג בדיקה' },
    { key: 'notes', label: 'הערות' },
    { key: 'checkedAt', label: 'תאריך בדיקה' },
  ];

  tableData: Record<string, string>[] = [];

  constructor(
    private route: ActivatedRoute,
    private thresholdService: ThresholdCheckService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.candidacyId = Number(this.route.snapshot.paramMap.get('candidacyId'));
    this.loadResults();
  }

  loadResults(): void {
    this.loading = true;
    this.thresholdService.getResults(this.candidacyId).subscribe({
      next: (data: ThresholdCheckResult[]) => {
        this.results = data;
        this.summary = {
          candidacyId: this.candidacyId,
          allPassed: data.length > 0 && data.every((r: ThresholdCheckResult) => r.passed),
          results: data,
        };
        this.tableData = this.mapResultsToTableData(data);
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת תוצאות בדיקת סף');
        this.loading = false;
      },
    });
  }

  onEvaluateAll(): void {
    this.evaluating = true;
    this.thresholdService.evaluateAll(this.candidacyId).subscribe({
      next: (summary: ThresholdCheckSummary) => {
        this.summary = summary;
        this.results = [...summary.results];
        this.tableData = this.mapResultsToTableData(summary.results);
        this.evaluating = false;
        this.notification.success('בדיקת תנאי סף הושלמה');
      },
      error: () => {
        this.notification.error('שגיאה בהרצת בדיקת תנאי סף');
        this.evaluating = false;
      },
    });
  }

  getConditionLabel(type: string): string {
    return CONDITION_TYPE_LABELS[type] ?? type;
  }

  private mapResultsToTableData(results: ThresholdCheckResult[]): Record<string, string>[] {
    return results.map((r) => ({
      conditionType: this.getConditionLabel(r.conditionType),
      fieldName: r.fieldName,
      passed: r.passed ? '✓ עבר' : '✗ לא עבר',
      actualValue: r.actualValue || '-',
      isAutomatic: r.isAutomatic ? 'אוטומטית' : 'ידנית',
      notes: r.notes || '-',
      checkedAt: r.checkedAt,
    }));
  }
}
