import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { OrgSubUnitTree, OrgPosition } from '../../models/org-structure.models';
import { OrgStructureApiService } from '../../services/org-structure.service';
import { NotificationService } from '@core/services/notification.service';

@Component({
  selector: 'app-org-tree',
  template: `
    <div class="page-header">
      <h1>מבנה ארגוני</h1>
    </div>

    <igds-card>
      <div class="filter-row">
        <igds-input-field
          label="מזהה יחידה ארגונית"
          type="number"
          [formControl]="orgUnitId">
        </igds-input-field>

        <igds-button variant="primary" [iconBefore]="true" (onClick)="loadTree()">
          <span igds-icon-before>🌳</span>
          טען מבנה
        </igds-button>
      </div>

      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <div *ngIf="!loading && treeData.length > 0" class="tree-container">
        <ng-container *ngFor="let node of treeData">
          <ng-container *ngTemplateOutlet="treeNode; context: { $implicit: node, level: 0 }"></ng-container>
        </ng-container>
      </div>

      <div *ngIf="!loading && treeData.length === 0" class="no-data">
        <span class="no-data__icon">ℹ️</span>
        <p>לא נמצא מבנה ארגוני ליחידה זו</p>
      </div>
    </igds-card>

    <ng-template #treeNode let-node let-level="level">
      <div *ngIf="hasChildren(node); else leafUnit" class="tree-node" [style.padding-inline-start.px]="level * 24">
        <igds-accordion
          [title]="node.name"
          [expanded]="true">
          <span *ngIf="!node.isActive" class="inactive-badge" [igdsTooltip]="'לא פעיל'">⛔</span>
          <ng-container *ngFor="let child of node.children">
            <ng-container *ngTemplateOutlet="treeNode; context: { $implicit: child, level: level + 1 }"></ng-container>
          </ng-container>
          <div *ngFor="let pos of node.positions" class="tree-leaf" [style.padding-inline-start.px]="(level + 1) * 24">
            <span class="leaf-icon leaf-icon--position">💼</span>
            <span class="node-label">{{ pos.title }}</span>
            <span *ngIf="pos.maxOccupants" class="occupants-badge">({{ pos.maxOccupants }} משרות)</span>
            <span *ngIf="!pos.isActive" class="inactive-badge" [igdsTooltip]="'לא פעיל'">⛔</span>
          </div>
        </igds-accordion>
      </div>

      <ng-template #leafUnit>
        <div class="tree-leaf" [style.padding-inline-start.px]="level * 24">
          <span class="leaf-icon leaf-icon--unit">📁</span>
          <span class="node-label">{{ node.name }}</span>
          <span *ngIf="!node.isActive" class="inactive-badge" [igdsTooltip]="'לא פעיל'">⛔</span>
          <div *ngFor="let pos of node.positions" class="tree-leaf tree-leaf--nested">
            <span class="leaf-icon leaf-icon--position">💼</span>
            <span class="node-label">{{ pos.title }}</span>
            <span *ngIf="pos.maxOccupants" class="occupants-badge">({{ pos.maxOccupants }} משרות)</span>
            <span *ngIf="!pos.isActive" class="inactive-badge" [igdsTooltip]="'לא פעיל'">⛔</span>
          </div>
        </div>
      </ng-template>
    </ng-template>
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
    .filter-row {
      display: flex;
      gap: var(--igds-space-16);
      align-items: flex-end;
      margin-block-end: var(--igds-space-16);
    }
    .tree-container {
      display: flex;
      flex-direction: column;
      gap: var(--igds-space-8);
    }
    .tree-node {
      margin-block-end: var(--igds-space-4);
    }
    .tree-leaf {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      padding: var(--igds-space-8) var(--igds-space-12);
      min-height: 44px;
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .tree-leaf--nested {
      padding-inline-start: var(--igds-space-24);
    }
    .leaf-icon {
      font-size: var(--igds-font-size-lg);
      flex-shrink: 0;
    }
    .node-label {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary);
    }
    .occupants-badge {
      font-size: var(--igds-font-size-xs);
      color: var(--igds-text-secondary);
      margin-inline-start: var(--igds-space-4);
    }
    .inactive-badge {
      font-size: var(--igds-font-size-sm);
      margin-inline-start: var(--igds-space-4);
      cursor: default;
    }
    .no-data {
      text-align: center;
      padding: var(--igds-space-48);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .no-data__icon {
      font-size: var(--igds-font-size-3xl);
      display: block;
      margin-block-end: var(--igds-space-8);
    }
  `],
})
export class OrgTreeComponent implements OnInit {
  orgUnitId = new FormControl<number | null>(1);
  loading = false;
  treeData: OrgSubUnitTree[] = [];

  constructor(
    private orgStructureApi: OrgStructureApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.loadTree();
  }

  hasChildren(node: OrgSubUnitTree): boolean {
    return (node.children && node.children.length > 0) ||
           (node.positions && node.positions.length > 0 && node.children && node.children.length > 0);
  }

  loadTree(): void {
    this.loading = true;
    const id = this.orgUnitId.value ?? 1;

    this.orgStructureApi.getTree(id).subscribe({
      next: (tree: OrgSubUnitTree) => {
        this.treeData = tree.children || [];
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת מבנה ארגוני');
        this.loading = false;
      },
    });
  }
}
