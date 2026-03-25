import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { QuotaFulfillment, OrgUnitFulfillment } from '../../models/quota.models';
import { QuotaApiService } from '../../services/quota.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-quota-fulfillment',
  template: `
    <div class="page-header">
      <h1>מצב מילוי מכסות</h1>
    </div>

    <igds-card>
      <div igds-card-body>
        <div class="filter-row">
          <igds-input-field
            label="מזהה יחידה ארגונית"
            type="number"
            [formControl]="orgUnitId">
          </igds-input-field>

          <igds-button variant="primary" (onClick)="onLoad()">
            הצג מצב מילוי
          </igds-button>
        </div>

        <app-loading-spinner [loading]="loading"></app-loading-spinner>

        <div *ngIf="!loading && fulfillment" class="fulfillment-list">
          <igds-card *ngFor="let quota of fulfillment.quotas" class="quota-card">
            <div igds-card-header>
              <h3 class="quota-title">{{ quota.categoryName }}</h3>
              <p class="quota-subtitle">
                {{ quota.currentCount }} / {{ quota.targetCount }}
                ({{ quota.fulfillmentPercentage | number:'1.0-0' }}%)
              </p>
            </div>
            <div igds-card-body>
              <igds-progress-bar
                [value]="quota.fulfillmentPercentage"
                [max]="100">
              </igds-progress-bar>
              <div class="status-row">
                <igds-status-badge
                  [variant]="getStatusVariant(quota.fulfillmentPercentage)"
                  [label]="getStatusLabel(quota.fulfillmentPercentage)">
                </igds-status-badge>
                <igds-tag *ngIf="!quota.isActive" label="לא פעילה" variant="neutral"></igds-tag>
              </div>
            </div>
          </igds-card>

          <div *ngIf="fulfillment.quotas.length === 0" class="no-data">
            <p>לא הוגדרו מכסות ליחידה זו</p>
          </div>
        </div>
      </div>
    </igds-card>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header { margin-block-end: var(--igds-space-16); }
    .page-header h1 {
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .filter-row {
      display: flex;
      gap: var(--igds-space-16);
      align-items: center;
      margin-block-end: var(--igds-space-16);
    }
    .fulfillment-list {
      display: flex;
      flex-direction: column;
      gap: var(--igds-space-16);
    }
    .quota-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .quota-subtitle {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-secondary);
      margin: var(--igds-space-4) 0 0;
    }
    .status-row {
      display: flex;
      gap: var(--igds-space-8);
      align-items: center;
      margin-block-start: var(--igds-space-12);
    }
    .no-data {
      text-align: center;
      padding: var(--igds-space-48);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
  `],
})
export class QuotaFulfillmentComponent implements OnInit {
  fulfillment: OrgUnitFulfillment | null = null;
  loading = false;

  orgUnitId = new FormControl<number | null>(1);

  constructor(
    private quotaApi: QuotaApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.onLoad();
  }

  onLoad(): void {
    this.loading = true;
    const id = this.orgUnitId.value ?? 1;

    this.quotaApi.getFulfillmentStatus(id).subscribe({
      next: (data: OrgUnitFulfillment) => {
        this.fulfillment = data;
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת מצב מילוי מכסות');
        this.loading = false;
      },
    });
  }

  getStatusVariant(percentage: number): string {
    if (percentage >= 80) return 'success';
    if (percentage >= 50) return 'warning';
    return 'failure';
  }

  getStatusLabel(percentage: number): string {
    if (percentage >= 80) return 'מילוי גבוה';
    if (percentage >= 50) return 'מילוי בינוני';
    return 'מילוי נמוך';
  }
}
