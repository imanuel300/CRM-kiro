import { Component, Input } from '@angular/core';

@Component({
  selector: 'igds-progress-bar',
  template: `
    <div class="igds-progress" role="progressbar"
      [attr.aria-valuenow]="value" aria-valuemin="0" aria-valuemax="100"
      [attr.aria-label]="'התקדמות: ' + value + '%'">
      <div class="igds-progress__track">
        <div class="igds-progress__fill" [ngClass]="'igds-progress__fill--' + variant"
          [style.width.%]="clampedValue"></div>
      </div>
      <span class="igds-progress__label">{{clampedValue}}%</span>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-progress { font-family: var(--igds-font-family); display: flex; align-items: center; gap: var(--igds-space-8); }
    .igds-progress__track {
      flex: 1; height: 8px; background: var(--igds-bg-neutral-secondlevel);
      border-radius: var(--igds-radius-full); overflow: hidden;
    }
    .igds-progress__fill {
      height: 100%; border-radius: var(--igds-radius-full);
      transition: width var(--igds-transition-normal);
    }
    .igds-progress__fill--default { background: var(--igds-bg-brand-default); }
    .igds-progress__fill--success { background: var(--igds-bg-success-without-text); }
    .igds-progress__fill--warning { background: var(--igds-bg-warning-without-text); }
    .igds-progress__fill--failure { background: var(--igds-bg-failure-without-text); }
    .igds-progress__label { font-size: var(--igds-font-size-xs); color: var(--igds-text-secondary); min-width: 36px; text-align: end; }
  `]
})
export class IgdsProgressBarComponent {
  @Input() value = 0;
  @Input() max = 100;
  @Input() variant: 'default' | 'success' | 'warning' | 'failure' = 'default';

  get clampedValue(): number { return Math.max(0, Math.min(100, this.max > 0 ? (this.value / this.max) * 100 : 0)); }
}
