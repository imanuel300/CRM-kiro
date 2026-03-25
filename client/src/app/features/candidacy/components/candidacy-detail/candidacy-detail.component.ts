import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { CandidacyDetail } from '../../models/candidacy.models';
import { CandidacyService } from '../../services/candidacy.service';
import { NotificationService } from '@core/services/notification.service';
import { Candidacy } from '../../models/candidacy.models';
import { IgdsTab } from '@igds/angular';
import { IgdsTableColumn } from '@igds/angular';

@Component({
  selector: 'app-candidacy-detail',
  template: `
    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div *ngIf="!loading && candidacy">
      <div class="page-header">
        <h1>מועמדות מספר {{ candidacy.id }}</h1>
        <div class="header-actions">
          <igds-button variant="secondary" (onClick)="onBack()">חזרה לרשימה</igds-button>
        </div>
      </div>

      <igds-status-badge
        [variant]="candidacy.isActive ? 'success' : 'neutral'"
        [text]="candidacy.isActive ? 'פעילה' : 'לא פעילה'">
      </igds-status-badge>

      <igds-tabs [tabs]="tabs" [activeTab]="activeTab" (tabChange)="onTabChange($event)"></igds-tabs>

      <div class="tab-content" *ngIf="activeTab === 'details'">
        <igds-card>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="label">מזהה מועמדות</span>
              <span class="value">{{ candidacy.id }}</span>
            </div>
            <div class="detail-item">
              <span class="label">מזהה איש קשר</span>
              <span class="value">
                <a [routerLink]="['/contacts', candidacy.contactId]">{{ candidacy.contactId }}</a>
              </span>
            </div>
            <div class="detail-item">
              <span class="label">יחידה ארגונית</span>
              <span class="value">{{ candidacy.orgUnitId }}</span>
            </div>
            <div class="detail-item">
              <span class="label">קול קורא</span>
              <span class="value">{{ candidacy.callForCandidatesId }}</span>
            </div>
            <div class="detail-item">
              <span class="label">סטטוס נוכחי</span>
              <span class="value">{{ candidacy.currentStatusId ?? '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">תת-סטטוס</span>
              <span class="value">{{ candidacy.currentSubStatusId ?? '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">גרסת תהליך</span>
              <span class="value">{{ candidacy.workflowDefinitionVersion ?? '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">תאריך הגשה</span>
              <span class="value">{{ candidacy.submittedAt ? (candidacy.submittedAt | hebrewDate) : '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">נוצר בתאריך</span>
              <span class="value">{{ candidacy.createdAt | hebrewDate }}</span>
            </div>
            <div class="detail-item">
              <span class="label">עודכן לאחרונה</span>
              <span class="value">{{ candidacy.updatedAt | hebrewDate }}</span>
            </div>
          </div>
        </igds-card>
      </div>

      <div class="tab-content" *ngIf="activeTab === 'history'">
        <igds-card>
          <app-status-timeline [candidacyId]="candidacy.id"></app-status-timeline>
        </igds-card>
      </div>

      <div class="tab-content" *ngIf="activeTab === 'custom' && candidacy.customFields?.length">
        <igds-card>
          <igds-table
            [columns]="customFieldColumns"
            [data]="candidacy.customFields">
          </igds-table>
        </igds-card>
      </div>

      <div class="tab-content" *ngIf="activeTab === 'related'">
        <igds-card>
          <app-loading-spinner [loading]="loadingRelated"></app-loading-spinner>

          <igds-table
            *ngIf="!loadingRelated"
            [columns]="relatedColumns"
            [data]="relatedTableData">
          </igds-table>

          <div class="no-data" *ngIf="!loadingRelated && relatedCandidacies.length === 0">
            לא נמצאו מועמדויות נוספות
          </div>
        </igds-card>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-block-end: var(--igds-space-8);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .header-actions {
      display: flex;
      gap: var(--igds-space-8);
    }
    .tab-content {
      margin-block-start: var(--igds-space-16);
    }
    .detail-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--igds-space-16);
    }
    .detail-item {
      display: flex;
      flex-direction: column;
    }
    .label {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xs);
      color: var(--igds-text-secondary);
      margin-block-end: var(--igds-space-4);
    }
    .value {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary);
    }
    .value a {
      color: var(--igds-text-link-default);
      text-decoration: none;
    }
    .value a:hover {
      color: var(--igds-text-link-hover);
      text-decoration: underline;
    }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
  `],
})
export class CandidacyDetailComponent implements OnInit {
  candidacy: CandidacyDetail | null = null;
  relatedCandidacies: Candidacy[] = [];
  loading = false;
  loadingRelated = false;
  activeTab = 'details';

  tabs: IgdsTab[] = [
    { id: 'details', label: 'פרטי מועמדות' },
    { id: 'history', label: 'היסטוריית סטטוסים' },
    { id: 'custom', label: 'שדות מותאמים' },
    { id: 'related', label: 'מועמדויות נוספות של איש הקשר' },
  ];

  customFieldColumns: IgdsTableColumn[] = [
    { key: 'fieldName', label: 'שם שדה' },
    { key: 'fieldType', label: 'סוג' },
    { key: 'value', label: 'ערך' },
  ];

  relatedColumns: IgdsTableColumn[] = [
    { key: 'id', label: 'מזהה' },
    { key: 'orgUnitId', label: 'יחידה ארגונית' },
    { key: 'callForCandidatesId', label: 'קול קורא' },
    { key: 'isActiveLabel', label: 'פעילה' },
    { key: 'submittedAtFormatted', label: 'תאריך הגשה' },
  ];

  get relatedTableData(): any[] {
    return this.relatedCandidacies.map(c => ({
      ...c,
      isActiveLabel: c.isActive ? '✓' : '✗',
      submittedAtFormatted: c.submittedAt || '—',
    }));
  }

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private candidacyService: CandidacyService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadCandidacy(+id);
    }
  }

  onTabChange(tabId: string): void {
    this.activeTab = tabId;
  }

  onBack(): void {
    this.router.navigate(['/candidacies']);
  }

  private loadCandidacy(id: number): void {
    this.loading = true;
    this.candidacyService.getDetail(id).subscribe({
      next: (detail: CandidacyDetail) => {
        this.candidacy = detail;
        this.loading = false;
        this.loadRelatedCandidacies(detail.contactId, detail.id);
      },
      error: () => {
        this.notification.error('שגיאה בטעינת פרטי המועמדות');
        this.loading = false;
      },
    });
  }

  private loadRelatedCandidacies(contactId: number, currentId: number): void {
    this.loadingRelated = true;
    this.candidacyService.list({ contactId }).subscribe({
      next: (data: Candidacy[]) => {
        this.relatedCandidacies = data.filter(c => c.id !== currentId);
        this.loadingRelated = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת מועמדויות קשורות');
        this.loadingRelated = false;
      },
    });
  }
}
