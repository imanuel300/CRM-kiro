import { Component, Input } from '@angular/core';

export interface IgdsStep {
  label: string;
  completed?: boolean;
}

@Component({
  selector: 'igds-step-indicator',
  template: `
    <nav class="igds-steps" aria-label="שלבי תהליך">
      <ol class="igds-steps__list">
        <li *ngFor="let step of steps; let i = index" class="igds-steps__item"
          [class.igds-steps__item--completed]="step.completed"
          [class.igds-steps__item--active]="i === activeStep"
          [attr.aria-current]="i === activeStep ? 'step' : null">
          <span class="igds-steps__circle">
            <span *ngIf="step.completed">✓</span>
            <span *ngIf="!step.completed">{{i + 1}}</span>
          </span>
          <span class="igds-steps__label">{{step.label}}</span>
          <span *ngIf="i < steps.length - 1" class="igds-steps__line"></span>
        </li>
      </ol>
    </nav>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-steps { font-family: var(--igds-font-family); }
    .igds-steps__list {
      display: flex; align-items: flex-start; list-style: none; margin: 0; padding: 0;
    }
    .igds-steps__item {
      display: flex; flex-direction: column; align-items: center; flex: 1;
      position: relative; text-align: center;
    }
    .igds-steps__circle {
      width: 32px; height: 32px; border-radius: var(--igds-radius-full);
      display: flex; align-items: center; justify-content: center;
      font-size: var(--igds-font-size-sm); font-weight: var(--igds-font-weight-bold);
      border: 2px solid var(--igds-border-subtle-default); background: var(--igds-bg-neutral);
      color: var(--igds-text-secondary); transition: all var(--igds-transition-fast);
      position: relative; z-index: 1;
    }
    .igds-steps__item--active .igds-steps__circle {
      border-color: var(--igds-bg-brand-default); background: var(--igds-bg-brand-default);
      color: var(--igds-text-inverted);
    }
    .igds-steps__item--completed .igds-steps__circle {
      border-color: var(--igds-bg-success-without-text); background: var(--igds-bg-success-without-text);
      color: var(--igds-text-inverted);
    }
    .igds-steps__label {
      margin-top: var(--igds-space-4); font-size: var(--igds-font-size-xs);
      color: var(--igds-text-secondary);
    }
    .igds-steps__item--active .igds-steps__label { color: var(--igds-text-primary); font-weight: var(--igds-font-weight-medium); }
    .igds-steps__line {
      position: absolute; top: 16px; inset-inline-start: calc(50% + 20px);
      width: calc(100% - 40px); height: 2px;
      background: var(--igds-border-divider);
    }
    .igds-steps__item--completed .igds-steps__line { background: var(--igds-bg-success-without-text); }
  `]
})
export class IgdsStepIndicatorComponent {
  @Input() steps: IgdsStep[] = [];
  @Input() activeStep = 0;
}
