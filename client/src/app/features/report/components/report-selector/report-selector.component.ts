import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  ReportType,
  REPORT_TYPES,
  StatusReportDto,
  CrossUnitReportDto,
  ReportResult,
  CustomReportDefinitionDto,
} from '../../models/report.models';
import { ReportService } from '../../services/report.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-report-selector',
  template: `
    <div class="page-header">
      <h1>הפקת דוחות</h1>
    </div>

    <igds-card>
      <form [formGroup]="form" (ngSubmit)="onGenerate()">
        <div class="form-row">
          <igds-dropdown
            label="סוג דוח"
            formControlName="reportType"
            [options]="reportTypeOptions"
            class="form-field">
          </igds-dropdown>

          <igds-input-field
            *ngIf="form.value.reportType !== 'cross-unit'"
            label="יחידה ארגונית"
            type="number"
            formControlName="orgUnitId"
            class="form-field">
          </igds-input-field>
        </div>

        <div class="form-row">
          <igds-date-picker
            label="מתאריך"
            formControlName="fromDate"
            class="form-field">
          </igds-date-picker>

          <igds-date-picker
            label="עד תאריך"
            formControlName="toDate"
            class="form-field">
          </igds-date-picker>
        </div>

        <div class="form-row" *ngIf="form.value.reportType === 'status'">
          <igds-input-field
            label="קול קורא (מזהה)"
            type="number"
            formControlName="callForCandidatesId"
            class="form-field">
          </igds-input-field>

          <igds-input-field
            label="סטטוס"
            formControlName="statusCode"
            class="form-field">
          </igds-input-field>
        </div>

        <div class="form-row" *ngIf="form.value.reportType === 'custom'">
          <igds-dropdown
            label="דוח מותאם"
            formControlName="customReportDefinitionId"
            [options]="customDefinitionOptions"
            class="form-field">
          </igds-dropdown>
        </div>

        <div class="actions-row">
          <igds-button variant="primary" type="submit" [disabled]="loading || form.invalid">
            הפק דוח
          </igds-button>
        </div>
      </form>
    </igds-card>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <app-report-results
      *ngIf="statusReport || crossUnitReport || customReport"
      [statusReport]="statusReport"
      [crossUnitReport]="crossUnitReport"
      [customReport]="customReport"
      [reportType]="form.value.reportType"
      (exportRequested)="onExport()">
    </app-report-results>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header { margin-bottom: var(--igds-space-16); }
    .form-row {
      display: flex;
      gap: var(--igds-space-16);
      flex-wrap: wrap;
      margin-bottom: var(--igds-space-8);
    }
    .form-field { flex: 1; min-width: 200px; }
    .actions-row { margin-top: var(--igds-space-16); }
  `],
})
export class ReportSelectorComponent implements OnInit {
  reportTypes = REPORT_TYPES;
  reportTypeOptions: IgdsDropdownOption[] = REPORT_TYPES.map(rt => ({ value: rt.value, label: rt.label }));
  form!: FormGroup;
  loading = false;
  customDefinitions: CustomReportDefinitionDto[] = [];
  customDefinitionOptions: IgdsDropdownOption[] = [];

  statusReport: StatusReportDto | null = null;
  crossUnitReport: CrossUnitReportDto | null = null;
  customReport: ReportResult | null = null;

  constructor(
    private fb: FormBuilder,
    private reportService: ReportService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      reportType: ['status', Validators.required],
      orgUnitId: [null],
      fromDate: [null],
      toDate: [null],
      callForCandidatesId: [null],
      statusCode: [null],
      customReportDefinitionId: [null],
    });

    this.form.get('reportType')!.valueChanges.subscribe(() => this.onReportTypeChange());
  }

  onReportTypeChange(): void {
    this.statusReport = null;
    this.crossUnitReport = null;
    this.customReport = null;

    if (this.form.value.reportType === 'custom' && this.form.value.orgUnitId) {
      this.loadCustomDefinitions();
    }
  }

  loadCustomDefinitions(): void {
    const orgUnitId = this.form.value.orgUnitId;
    if (!orgUnitId) return;
    this.reportService.getCustomReportDefinitions(orgUnitId).subscribe({
      next: (defs: CustomReportDefinitionDto[]) => {
        this.customDefinitions = defs;
        this.customDefinitionOptions = defs.map(d => ({ value: d.id, label: d.name }));
      },
      error: () => this.notification.error('שגיאה בטעינת הגדרות דוחות מותאמים'),
    });
  }

  onGenerate(): void {
    const v = this.form.value;
    this.loading = true;
    this.statusReport = null;
    this.crossUnitReport = null;
    this.customReport = null;

    const formatDate = (d: Date | null): string | undefined =>
      d ? d.toISOString() : undefined;

    switch (v.reportType as ReportType) {
      case 'status':
        this.reportService
          .getStatusReport({
            orgUnitId: v.orgUnitId,
            callForCandidatesId: v.callForCandidatesId || undefined,
            statusCode: v.statusCode || undefined,
            fromDate: formatDate(v.fromDate),
            toDate: formatDate(v.toDate),
          })
          .subscribe({
            next: (data: StatusReportDto) => { this.statusReport = data; this.loading = false; },
            error: () => { this.notification.error('שגיאה בהפקת דוח'); this.loading = false; },
          });
        break;

      case 'cross-unit':
        this.reportService
          .getCrossUnitReport({
            fromDate: formatDate(v.fromDate),
            toDate: formatDate(v.toDate),
          })
          .subscribe({
            next: (data: CrossUnitReportDto) => { this.crossUnitReport = data; this.loading = false; },
            error: () => { this.notification.error('שגיאה בהפקת דוח'); this.loading = false; },
          });
        break;

      case 'custom':
        this.reportService
          .getCustomReport({
            orgUnitId: v.orgUnitId,
            customReportDefinitionId: v.customReportDefinitionId,
            fromDate: formatDate(v.fromDate),
            toDate: formatDate(v.toDate),
          })
          .subscribe({
            next: (data: ReportResult) => { this.customReport = data; this.loading = false; },
            error: () => { this.notification.error('שגיאה בהפקת דוח'); this.loading = false; },
          });
        break;
    }
  }

  onExport(): void {
    const v = this.form.value;
    const formatDate = (d: Date | null): string | undefined =>
      d ? d.toISOString() : undefined;

    this.reportService
      .exportToExcel({
        reportType: v.reportType,
        orgUnitId: v.orgUnitId || undefined,
        callForCandidatesId: v.callForCandidatesId || undefined,
        statusCode: v.statusCode || undefined,
        fromDate: formatDate(v.fromDate),
        toDate: formatDate(v.toDate),
      })
      .subscribe({
        next: (blob: Blob) => {
          const url = window.URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `report-${v.reportType}-${new Date().toISOString().slice(0, 10)}.csv`;
          a.click();
          window.URL.revokeObjectURL(url);
          this.notification.success('הדוח יוצא בהצלחה');
        },
        error: () => this.notification.error('שגיאה בייצוא דוח'),
      });
  }
}
