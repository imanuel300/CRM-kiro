import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { SubUnitOccupancy, PositionOccupancy } from '../../models/org-structure.models';
import { OrgStructureApiService } from '../../services/org-structure.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-occupancy-view',
  template: `
    <div class="page-header">
      <h1>תפוסת תפקידים</h1>
    </div>

    <igds-card>
      <div igds-card-body>
        <div class="filter-row">
          <igds-input-field
            label="מזהה יחידת משנה"
            type="number"
            [formControl]="subUnitId">
          </igds-input-field>

          <igds-button variant="primary" (onClick)="loadOccupancy()">
            הצג תפוסה
          </igds-button>
        </div>

        <app-loading-spinner [loading]="loading"></app-loading-spinner>

        <div *ngIf="!loading && occupancy" class="occupancy-list">
          <h2 class="section-title">{{ occupancy.subUnitName }}</h2>

          <igds-card *ngFor="let pos of occupancy.positions" class="position-card">
            <div igds-card-header>
              <h3 class="pos-title">{{ pos.title }}</h3>
              <p class="pos-subtitle">
                {{ pos.filledCount }} / {{ pos.maxOccupants }} מאוישות
                ({{ getPercentage(pos) | number:'1.0-0' }}%)
              </p>
            </div>
            <div igds-card-body>
              <igds-progress-bar
                [value]="getPercentage(pos)"
                [max]="100">
              </igds-progress-bar>
              <div class="status-row">
                <igds-status-badge
                  [variant]="getStatusVariant(pos)"
                  [label]="pos.vacantCount > 0 ? pos.vacantCount + ' משרות פנויות' : 'מאויש במלואו'">
                </igds-status-badge>
              </div>
            </div>
          </igds-card>

          <div *ngIf="occupancy.positions.length === 0" class="no-data">
            <p>לא הוגדרו תפקידים ליחידת משנה זו</p>
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
    .section-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-lg);
      color: var(--igds-text-primary);
    }
    .occupancy-list {
      display: flex;
      flex-direction: column;
      gap: var(--igds-space-16);
    }
    .pos-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .pos-subtitle {
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
export class OccupancyViewComponent implements OnInit {
  occupancy: SubUnitOccupancy | null = null;
  loading = false;

  subUnitId = new FormControl<number | null>(1);

  constructor(
    private orgStructureApi: OrgStructureApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadOccupancy();
  }

  loadOccupancy(): void {
    this.loading = true;
    const id = this.subUnitId.value ?? 1;

    this.orgStructureApi.getOccupancy(id).subscribe({
      next: (data) => {
        this.occupancy = data;
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת תפוסת תפקידים');
        this.loading = false;
      },
    });
  }

  getPercentage(pos: PositionOccupancy): number {
    if (pos.maxOccupants === 0) return 0;
    return (pos.filledCount / pos.maxOccupants) * 100;
  }

  getStatusVariant(pos: PositionOccupancy): string {
    if (pos.vacantCount === 0) return 'success';
    if (pos.filledCount > 0) return 'warning';
    return 'failure';
  }
}
