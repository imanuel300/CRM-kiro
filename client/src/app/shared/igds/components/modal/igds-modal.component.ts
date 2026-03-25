import { Component, Input, Output, EventEmitter, HostListener } from '@angular/core';

@Component({
  selector: 'igds-modal',
  template: `
    <div *ngIf="visible" class="igds-modal__overlay" (click)="onOverlayClick($event)">
      <div class="igds-modal" role="dialog" aria-modal="true" [attr.aria-label]="title"
        (keydown.escape)="onClose()">
        <div class="igds-modal__header">
          <h2 class="igds-modal__title">{{title}}</h2>
          <button *ngIf="closable" class="igds-modal__close" type="button"
            aria-label="סגור" (click)="onClose()">
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor"
              stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <div class="igds-modal__body"><ng-content></ng-content></div>
        <div class="igds-modal__footer"><ng-content select="[igds-modal-footer]"></ng-content></div>
      </div>
    </div>
  `,
  styles: [`
    :host { direction: inherit; }
    .igds-modal__overlay {
      position: fixed; inset: 0; z-index: 9000;
      background: rgba(0, 0, 0, 0.5); display: flex; align-items: center; justify-content: center;
      animation: igds-fade-in var(--igds-transition-fast);
    }
    @keyframes igds-fade-in { from { opacity: 0; } to { opacity: 1; } }
    .igds-modal {
      background: var(--igds-bg-neutral); border-radius: var(--igds-radius-lg);
      box-shadow: var(--igds-shadow-xl); width: 90%; max-width: 560px; max-height: 90vh;
      display: flex; flex-direction: column; font-family: var(--igds-font-family);
      animation: igds-modal-in var(--igds-transition-normal);
    }
    @keyframes igds-modal-in { from { opacity: 0; transform: scale(0.95); } to { opacity: 1; transform: scale(1); } }
    .igds-modal__header {
      display: flex; align-items: center; justify-content: space-between;
      padding: var(--igds-space-16) var(--igds-space-24);
      border-bottom: 1px solid var(--igds-border-divider);
    }
    .igds-modal__title { margin: 0; font-size: var(--igds-font-size-xl); font-weight: var(--igds-font-weight-bold); color: var(--igds-text-primary); }
    .igds-modal__close {
      display: flex; background: none; border: none; cursor: pointer; padding: var(--igds-space-4);
      color: var(--igds-text-secondary); border-radius: var(--igds-radius-full);
    }
    .igds-modal__close:hover { color: var(--igds-text-primary); }
    .igds-modal__close:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    .igds-modal__body { padding: var(--igds-space-24); overflow-y: auto; flex: 1; color: var(--igds-text-primary); }
    .igds-modal__footer {
      padding: var(--igds-space-16) var(--igds-space-24);
      border-top: 1px solid var(--igds-border-divider);
      display: flex; justify-content: flex-end; gap: var(--igds-space-8);
    }
  `]
})
export class IgdsModalComponent {
  @Input() title = '';
  @Input() visible = false;
  @Input() closable = true;
  @Output() closed = new EventEmitter<void>();

  onClose() {
    if (this.closable) {
      this.visible = false;
      this.closed.emit();
    }
  }

  onOverlayClick(event: MouseEvent) {
    if (event.target === event.currentTarget && this.closable) { this.onClose(); }
  }
}
