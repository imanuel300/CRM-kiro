import { Component, Input, Output, EventEmitter } from '@angular/core';
import {
  StatusReportDto,
  CrossUnitReportDto,
  ReportResult,
  ReportType,
} from '../../models/report.models';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-report-results',
  template: `
    <igds-card class="results-card">
      <div igds-card-header class="results-header">
        <h2 class="results-title">תוצאות דוח</h2>
        <igds-button variant="secondary" (onClick)="exportRequested.emit()" ariaLabel="ייצוא ל-Excel">
          ייצוא ל-Excel
        </igds-button>
      </div>

      <!-- Status Report -->
      <ng-container *ngIf="reportType === 'status' && statusReport">
        <h3>{{ statusReport.orgUnitName }} — סה"כ {{ statusReport.totalCandidacies }} מועמדויות</h3>

        <h4>פילוח לפי סטטוס</h4>
        <igds-table
          [columns]="statusByStatusColumns"
          [data]="statusReport.byStatus">
        </igds-table>

        <ng-container *ngIf="statusReport.byCall.length">
          <h4>פילוח לפי קול קורא</h4>
          <igds-table
            [columns]="statusByCallColumns"
            [data]="statusReport.byCall">
          </igds-table>
        </ng-container>
      </ng-container>

      <!-- Cross-Unit Report -->
      <ng-container *ngIf="reportType === 'cross-unit' && crossUnitReport">
        <h3>דוח מאוחד — סה"כ {{ crossUnitReport.totalCandidacies }} מועמדויות</h3>
        <p class="generated-at">הופק: {{ crossUnitReport.generatedAt | hebrewDate }}</p>

        <igds-table
          [columns]="crossUnitColumns"
          [data]="crossUnitReport.units">
        </igds-table>
      </ng-container>

      <!-- Custom Report -->
      <ng-container *ngIf="reportType === 'custom' && customReport">
        <h3>{{ customReport.reportName }} — {{ customReport.totalRecords }} רשומות</h3>
        <p class="generated-at">הופק: {{ customReport.generatedAt | hebrewDate }}</p>

        <div *ngIf="customReport.aggregations.length" class="aggregations">
          <h4>סיכומים</h4>
          <igds-table
            [columns]="aggregationColumns"
            [data]="customReport.aggregations">
          </igds-table>
        </div>
      </ng-container>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .results-card { margin-top: var(--igds-space-16); }
    .results-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    .results-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-lg);
      font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .generated-at {
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm);
      margin-bottom: var(--igds-space-12);
    }
    .aggregations { margin-top: var(--igds-space-16); }
    h3 {
      margin-top: var(--igds-space-8);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    h4 {
      margin-top: var(--igds-space-16);
      margin-bottom: var(--igds-space-8);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    igds-table { margin-bottom: var(--igds-space-16); }
  `],
})
export class ReportResultsComponent {
  @Input() statusReport: StatusReportDto | null = null;
  @Input() crossUnitReport: CrossUnitReportDto | null = null;
  @Input() customReport: ReportResult | null = null;
  @Input() reportType: ReportType = 'status';
  @Output() exportRequested = new EventEmitter<void>();

  statusByStatusColumns: IgdsTableColumn[] = [
    { key: 'statusDisplayName', label: 'סטטוס' },
    { key: 'count', label: 'כמות' },
  ];

  statusByCallColumns: IgdsTableColumn[] = [
    { key: 'callTitle', label: 'קול קורא' },
    { key: 'totalCandidacies', label: 'סה"כ מועמדויות' },
  ];

  crossUnitColumns: IgdsTableColumn[] = [
    { key: 'orgUnitName', label: 'יחידה ארגונית' },
    { key: 'totalCandidacies', label: 'סה"כ' },
    { key: 'activeCandidacies', label: 'פעילות' },
  ];

  aggregationColumns: IgdsTableColumn[] = [
    { key: 'groupValue', label: 'קבוצה' },
    { key: 'count', label: 'כמות' },
  ];
}
