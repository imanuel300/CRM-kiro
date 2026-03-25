import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'igds-button',
  template: `
    <button
      [type]="type"
      [class]="'igds-btn igds-btn--' + variant + (iconOnly ? ' igds-btn--icon-only' : '')"
      [disabled]="disabled"
      [attr.aria-label]="ariaLabel"
      (click)="onClick.emit($event)">
      <span *ngIf="iconBefore" class="igds-btn__icon igds-btn__icon--before">
        <ng-content select="[igds-icon-before]"></ng-content>
      </span>
      <span class="igds-btn__label" *ngIf="!iconOnly">
        <ng-content></ng-content>
      </span>
      <span *ngIf="iconAfter" class="igds-btn__icon igds-btn__icon--after">
        <ng-content select="[igds-icon-after]"></ng-content>
      </span>
      <span *ngIf="iconOnly" class="igds-btn__icon">
        <ng-content select="[igds-icon]"></ng-content>
      </span>
    </button>
  `,
  styles: [`
    :host { display: inline-block; }
    .igds-btn {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      font-weight: var(--igds-font-weight-medium);
      line-height: 1.5;
      padding: var(--igds-space-8) var(--igds-space-24);
      border-radius: var(--igds-radius-md);
      border: 2px solid transparent;
      cursor: pointer;
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: var(--igds-space-8);
      transition: all var(--igds-transition-fast);
      min-height: 44px;
      direction: inherit;
    }
    .igds-btn:focus-visible {
      outline: 2px solid var(--igds-border-focused);
      outline-offset: 2px;
    }
    .igds-btn--primary {
      background: var(--igds-bg-brand-default);
      color: var(--igds-text-inverted);
      border-color: var(--igds-bg-brand-default);
    }
    .igds-btn--primary:hover:not(:disabled) {
      background: var(--igds-bg-brand-hover);
      border-color: var(--igds-bg-brand-hover);
    }
    .igds-btn--primary:active:not(:disabled) {
      background: var(--igds-bg-brand-pressed);
      border-color: var(--igds-bg-brand-pressed);
    }
    .igds-btn--secondary {
      background: var(--igds-bg-neutral);
      color: var(--igds-text-link-default);
      border-color: var(--igds-border-brand-default);
    }
    .igds-btn--secondary:hover:not(:disabled) {
      color: var(--igds-text-link-hover);
      border-color: var(--igds-border-brand-hover);
    }
    .igds-btn--secondary:active:not(:disabled) {
      background: var(--igds-bg-secondary-pressed);
    }
    .igds-btn--link {
      background: transparent;
      color: var(--igds-text-link-default);
      border: none;
      padding: var(--igds-space-4) var(--igds-space-8);
      text-decoration: underline;
    }
    .igds-btn--link:hover:not(:disabled) {
      color: var(--igds-text-link-hover);
    }
    .igds-btn:disabled {
      background: var(--igds-bg-disabled-with-border);
      color: var(--igds-text-disabled);
      border-color: var(--igds-border-disabled);
      cursor: not-allowed;
    }
    .igds-btn--link:disabled {
      background: transparent;
      border: none;
    }
    .igds-btn--icon-only { padding: var(--igds-space-8); }
    .igds-btn__icon { display: inline-flex; align-items: center; }
  `]
})
export class IgdsButtonComponent {
  @Input() variant: 'primary' | 'secondary' | 'link' = 'primary';
  @Input() type: 'button' | 'submit' | 'reset' = 'button';
  @Input() disabled = false;
  @Input() iconBefore = false;
  @Input() iconAfter = false;
  @Input() iconOnly = false;
  @Input() ariaLabel?: string;
  @Output() onClick = new EventEmitter<MouseEvent>();
}
