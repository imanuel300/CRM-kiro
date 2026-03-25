import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { OrgUnitService } from '../../services/org-unit.service';
import { NotificationService } from '@core/services/notification.service';
import { WorkflowDefinition } from '../../models/org-unit.models';

interface StepConfig {
  key: string;
  label: string;
  enabled: boolean;
}

@Component({
  selector: 'app-workflow-config',
  template: `
    <div class="page-header">
      <h1>הגדרת תהליך מיון</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <igds-card *ngIf="!loading">
      <div igds-card-header>
        <h2 class="card-title">שלבי מיון</h2>
        <p class="card-subtitle">גרור לשינוי סדר, הפעל/השבת שלבים</p>
      </div>

      <div class="step-list">
        <div
          *ngFor="let step of steps; let i = index"
          class="step-item"
          [class.disabled]="!step.enabled"
          draggable="true"
          (dragstart)="onDragStart(i)"
          (dragover)="onDragOver($event, i)"
          (drop)="onDrop($event, i)"
          (dragend)="onDragEnd()"
        >
          <span class="drag-handle">☰</span>
          <span class="step-order">{{ i + 1 }}</span>
          <span class="step-label">{{ step.label }}</span>
          <igds-toggle
            [checked]="step.enabled"
            [label]="step.enabled ? 'פעיל' : 'מושבת'"
            (click)="onToggleStep(step)">
          </igds-toggle>
        </div>
      </div>

      <div class="form-actions">
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
    .step-list {
      display: flex;
      flex-direction: column;
      gap: var(--igds-space-8);
      margin: var(--igds-space-16) 0;
    }
    .step-item {
      display: flex;
      align-items: center;
      gap: var(--igds-space-12);
      padding: var(--igds-space-12) var(--igds-space-16);
      border: 1px solid var(--igds-border-divider);
      border-radius: var(--igds-radius-md);
      background: var(--igds-bg-neutral);
      cursor: move;
      transition: box-shadow var(--igds-transition-fast);
    }
    .step-item:hover { box-shadow: var(--igds-shadow-sm); }
    .step-item.disabled { opacity: 0.6; background: var(--igds-bg-neutral-secondlevel); }
    .step-item.drag-over { border-color: var(--igds-border-focused); }
    .drag-handle { color: var(--igds-text-secondary); cursor: grab; }
    .step-order {
      width: 28px;
      height: 28px;
      border-radius: var(--igds-radius-full);
      background: var(--igds-bg-brand-default);
      color: var(--igds-text-inverted);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: var(--igds-font-weight-bold);
      font-size: var(--igds-font-size-sm);
      font-family: var(--igds-font-family);
    }
    .step-item.disabled .step-order { background: var(--igds-bg-disabled-without-border); }
    .step-label {
      flex: 1;
      font-size: var(--igds-font-size-md);
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
  `],
})
export class WorkflowConfigComponent implements OnInit {
  loading = false;
  saving = false;
  orgUnitId!: number;
  workflow?: WorkflowDefinition;
  private dragIndex: number | null = null;

  steps: StepConfig[] = [
    { key: 'thresholdCheck', label: 'בדיקת תנאי סף', enabled: true },
    { key: 'exam', label: 'מבחן', enabled: true },
    { key: 'interview', label: 'ראיון', enabled: true },
    { key: 'committee', label: 'ועדה', enabled: true },
  ];

  constructor(
    private route: ActivatedRoute,
    private orgUnitService: OrgUnitService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.orgUnitId = +this.route.snapshot.paramMap.get('id')!;
    this.loadWorkflow();
  }

  private loadWorkflow(): void {
    this.loading = true;
    this.orgUnitService.getWorkflow(this.orgUnitId).subscribe({
      next: (wf: WorkflowDefinition) => {
        this.workflow = wf;
        this.applyWorkflowToSteps(wf);
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      },
    });
  }

  private applyWorkflowToSteps(wf: WorkflowDefinition): void {
    const enabledMap: Record<string, boolean> = {
      thresholdCheck: wf.thresholdCheckEnabled,
      exam: wf.examStepEnabled,
      interview: wf.interviewStepEnabled,
      committee: wf.committeeStepEnabled,
    };
    this.steps.forEach((s) => (s.enabled = enabledMap[s.key] ?? s.enabled));

    if (wf.stepOrder) {
      const order = wf.stepOrder.split(',');
      this.steps.sort(
        (a, b) => order.indexOf(a.key) - order.indexOf(b.key)
      );
    }
  }

  onToggleStep(step: StepConfig): void {
    step.enabled = !step.enabled;
  }

  onDragStart(index: number): void {
    this.dragIndex = index;
  }

  onDragOver(event: DragEvent, index: number): void {
    event.preventDefault();
  }

  onDrop(event: DragEvent, targetIndex: number): void {
    event.preventDefault();
    if (this.dragIndex !== null && this.dragIndex !== targetIndex) {
      const item = this.steps.splice(this.dragIndex, 1)[0];
      this.steps.splice(targetIndex, 0, item);
    }
    this.dragIndex = null;
  }

  onDragEnd(): void {
    this.dragIndex = null;
  }

  onSave(): void {
    this.saving = true;
    const stepOrder = this.steps.map((s) => s.key).join(',');
    const enabledMap = Object.fromEntries(this.steps.map((s) => [s.key, s.enabled]));

    this.orgUnitService
      .configureWorkflow({
        orgUnitId: this.orgUnitId,
        name: this.workflow?.name || 'תהליך מיון',
        examStepEnabled: enabledMap['exam'] ?? false,
        interviewStepEnabled: enabledMap['interview'] ?? false,
        committeeStepEnabled: enabledMap['committee'] ?? false,
        thresholdCheckEnabled: enabledMap['thresholdCheck'] ?? false,
        stepOrder,
      })
      .subscribe({
        next: () => {
          this.notification.success('תהליך המיון עודכן בהצלחה');
          this.saving = false;
        },
        error: () => {
          this.notification.error('שגיאה בשמירת תהליך המיון');
          this.saving = false;
        },
      });
  }
}
