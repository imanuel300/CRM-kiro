import { Component, Input } from '@angular/core';

@Component({
  selector: 'igds-status-badge',
  template: `
    <span class="igds-badge" [ngClass]="'igds-badge--' + variant" role="status">
      <span class="igds-badge__dot"></span>
      {{text}}
    </span>
  `,
  styles: [`
    :host { display: inline-block; direction: inherit; }
    .igds-badge {
      display: inline-flex; align-items: center; gap: var(--igds-space-4);
      padding: var(--igds-space-2) var(--igds-space-8);
      border-radius: var(--igds-radius-full); font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xs); font-weight: var(--igds-font-weight-medium);
      line-height: 1.5; white-space: nowrap;
    }
    .igds-badge__dot { width: 8px; height: 8px; border-radius: var(--igds-radius-full); flex-shrink: 0; }
    .igds-badge--success { background: var(--igds-bg-success-with-border); color: var(--igds-text-success); border: 1px solid var(--igds-border-success); }
    .igds-badge--success .igds-badge__dot { background: var(--igds-bg-success-without-text); }
    .igds-badge--warning { background: var(--igds-bg-warning-with-border); color: var(--igds-text-warning); border: 1px solid var(--igds-border-warning); }
    .igds-badge--warning .igds-badge__dot { background: var(--igds-bg-warning-without-text); }
    .igds-badge--failure { background: var(--igds-bg-failure-with-border); color: var(--igds-text-failure); border: 1px solid var(--igds-border-failure); }
    .igds-badge--failure .igds-badge__dot { background: var(--igds-bg-failure-without-text); }
    .igds-badge--info { background: var(--igds-bg-secondary-pressed); color: var(--igds-text-link-default); border: 1px solid var(--igds-border-brand-default); }
    .igds-badge--info .igds-badge__dot { background: var(--igds-bg-brand-default); }
    .igds-badge--neutral { background: var(--igds-bg-neutral-secondlevel); color: var(--igds-text-secondary); border: 1px solid var(--igds-border-subtle-default); }
    .igds-badge--neutral .igds-badge__dot { background: var(--igds-border-subtle-default); }
  `]
})
export class IgdsStatusBadgeComponent {
  @Input() variant: 'success' | 'warning' | 'failure' | 'info' | 'neutral' = 'neutral';
  @Input() text = '';
}
