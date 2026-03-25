import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'igds-tag',
  template: `
    <span class="igds-tag" [ngClass]="'igds-tag--' + variant">
      <span class="igds-tag__label">{{label}}</span>
      <button *ngIf="removable" class="igds-tag__remove" type="button"
        [attr.aria-label]="'הסר ' + label" (click)="remove.emit()">
        <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor"
          stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
        </svg>
      </button>
    </span>
  `,
  styles: [`
    :host { display: inline-block; direction: inherit; }
    .igds-tag {
      display: inline-flex; align-items: center; gap: var(--igds-space-4);
      padding: var(--igds-space-2) var(--igds-space-8); border-radius: var(--igds-radius-full);
      font-family: var(--igds-font-family); font-size: var(--igds-font-size-xs);
      font-weight: var(--igds-font-weight-medium); line-height: 1.5;
    }
    .igds-tag--default { background: var(--igds-bg-neutral-secondlevel); color: var(--igds-text-primary); border: 1px solid var(--igds-border-subtle-default); }
    .igds-tag--success { background: var(--igds-bg-success-with-border); color: var(--igds-text-success); border: 1px solid var(--igds-border-success); }
    .igds-tag--warning { background: var(--igds-bg-warning-with-border); color: var(--igds-text-warning); border: 1px solid var(--igds-border-warning); }
    .igds-tag--failure { background: var(--igds-bg-failure-with-border); color: var(--igds-text-failure); border: 1px solid var(--igds-border-failure); }
    .igds-tag__remove {
      display: flex; align-items: center; background: none; border: none;
      cursor: pointer; padding: 0; color: inherit; border-radius: var(--igds-radius-full);
    }
    .igds-tag__remove:hover { opacity: 0.7; }
    .igds-tag__remove:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
  `]
})
export class IgdsTagComponent {
  @Input() label = '';
  @Input() removable = false;
  @Input() variant: 'default' | 'success' | 'warning' | 'failure' = 'default';
  @Output() remove = new EventEmitter<void>();
}
