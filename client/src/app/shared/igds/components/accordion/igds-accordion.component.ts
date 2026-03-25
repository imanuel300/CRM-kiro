import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'igds-accordion',
  template: `
    <div class="igds-accordion" [class.igds-accordion--expanded]="expanded" [class.igds-accordion--disabled]="disabled">
      <button class="igds-accordion__header" type="button"
        [disabled]="disabled"
        [attr.aria-expanded]="expanded"
        [attr.aria-controls]="panelId"
        (click)="toggle()">
        <span class="igds-accordion__title">{{title}}</span>
        <span class="igds-accordion__icon" aria-hidden="true">&#9662;</span>
      </button>
      <div *ngIf="expanded" class="igds-accordion__panel" [id]="panelId" role="region">
        <ng-content></ng-content>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-accordion {
      border: 1px solid var(--igds-border-divider); border-radius: var(--igds-radius-md);
      font-family: var(--igds-font-family); overflow: hidden;
    }
    .igds-accordion__header {
      display: flex; align-items: center; justify-content: space-between; width: 100%;
      padding: var(--igds-space-12) var(--igds-space-16); background: var(--igds-bg-neutral);
      border: none; cursor: pointer; font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md); font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary); transition: background var(--igds-transition-fast);
      min-height: 44px; text-align: inherit;
    }
    .igds-accordion__header:hover:not(:disabled) { background: var(--igds-bg-neutral-hover); }
    .igds-accordion__header:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: -2px; }
    .igds-accordion__header:disabled { color: var(--igds-text-disabled); cursor: not-allowed; }
    .igds-accordion__icon {
      transition: transform var(--igds-transition-fast); font-size: var(--igds-font-size-xs);
    }
    .igds-accordion--expanded .igds-accordion__icon { transform: rotate(180deg); }
    .igds-accordion__panel { padding: var(--igds-space-16); border-top: 1px solid var(--igds-border-divider); color: var(--igds-text-primary); }
  `]
})
export class IgdsAccordionComponent {
  @Input() title = '';
  @Input() expanded = false;
  @Input() disabled = false;
  @Output() toggled = new EventEmitter<boolean>();

  panelId = 'igds-accordion-panel-' + Math.random().toString(36).substr(2, 9);

  toggle() {
    if (this.disabled) return;
    this.expanded = !this.expanded;
    this.toggled.emit(this.expanded);
  }
}
