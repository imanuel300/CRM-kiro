import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'igds-drawer',
  template: `
    <div *ngIf="visible" class="igds-drawer__overlay" (click)="onClose()">
      <div class="igds-drawer" [class.igds-drawer--start]="position === 'start'"
        [class.igds-drawer--end]="position === 'end'"
        role="dialog" aria-modal="true" [attr.aria-label]="title"
        (click)="$event.stopPropagation()" (keydown.escape)="onClose()">
        <div class="igds-drawer__header">
          <h2 class="igds-drawer__title">{{title}}</h2>
          <button class="igds-drawer__close" type="button" aria-label="סגור"
            (click)="onClose()">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor"
              stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <div class="igds-drawer__body"><ng-content></ng-content></div>
      </div>
    </div>
  `,
  styles: [`
    :host { direction: inherit; }
    .igds-drawer__overlay {
      position: fixed; inset: 0; z-index: 9000;
      background: rgba(0, 0, 0, 0.5);
      animation: igds-fade-in var(--igds-transition-fast);
    }
    @keyframes igds-fade-in { from { opacity: 0; } to { opacity: 1; } }
    .igds-drawer {
      position: fixed; top: 0; bottom: 0; width: 360px; max-width: 85vw;
      background: var(--igds-bg-neutral); box-shadow: var(--igds-shadow-xl);
      display: flex; flex-direction: column; font-family: var(--igds-font-family);
      animation: igds-drawer-in var(--igds-transition-normal);
    }
    .igds-drawer--start { inset-inline-start: 0; }
    .igds-drawer--end { inset-inline-end: 0; }
    @keyframes igds-drawer-in { from { opacity: 0; transform: translateX(-20px); } to { opacity: 1; transform: translateX(0); } }
    .igds-drawer__header {
      display: flex; align-items: center; justify-content: space-between;
      padding: var(--igds-space-16) var(--igds-space-24);
      border-bottom: 1px solid var(--igds-border-divider);
    }
    .igds-drawer__title { margin: 0; font-size: var(--igds-font-size-xl); font-weight: var(--igds-font-weight-bold); color: var(--igds-text-primary); }
    .igds-drawer__close {
      display: flex; background: none; border: none; cursor: pointer; padding: var(--igds-space-4);
      color: var(--igds-text-secondary); border-radius: var(--igds-radius-full);
    }
    .igds-drawer__close:hover { color: var(--igds-text-primary); }
    .igds-drawer__close:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    .igds-drawer__body { padding: var(--igds-space-24); overflow-y: auto; flex: 1; }
  `]
})
export class IgdsDrawerComponent {
  @Input() visible = false;
  @Input() position: 'start' | 'end' = 'end';
  @Input() title = '';
  @Output() closed = new EventEmitter<void>();

  onClose() {
    this.visible = false;
    this.closed.emit();
  }
}
