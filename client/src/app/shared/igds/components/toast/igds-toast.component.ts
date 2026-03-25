import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges, OnDestroy } from '@angular/core';

@Component({
  selector: 'igds-toast',
  template: `
    <div *ngIf="visible" class="igds-toast" [ngClass]="'igds-toast--' + type"
      role="alert" aria-live="assertive" aria-atomic="true">
      <span class="igds-toast__icon" aria-hidden="true">
        <ng-container [ngSwitch]="type">
          <span *ngSwitchCase="'success'">✓</span>
          <span *ngSwitchCase="'warning'">⚠</span>
          <span *ngSwitchCase="'failure'">✕</span>
          <span *ngSwitchDefault>ℹ</span>
        </ng-container>
      </span>
      <span class="igds-toast__message">{{message}}</span>
      <button class="igds-toast__close" type="button" aria-label="סגור"
        (click)="close()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor"
          stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
        </svg>
      </button>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-toast {
      display: flex; align-items: center; gap: var(--igds-space-8);
      padding: var(--igds-space-12) var(--igds-space-16);
      border-radius: var(--igds-radius-md); font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm); box-shadow: var(--igds-shadow-lg);
      position: fixed; top: var(--igds-space-24); left: 50%; transform: translateX(-50%);
      z-index: 10000; min-width: 320px; max-width: 560px;
      animation: igds-toast-in var(--igds-transition-normal);
    }
    @keyframes igds-toast-in { from { opacity: 0; transform: translateX(-50%) translateY(-12px); } to { opacity: 1; transform: translateX(-50%) translateY(0); } }
    .igds-toast--success { background: var(--igds-bg-success-with-border); color: var(--igds-text-success); border: 1px solid var(--igds-border-success); }
    .igds-toast--warning { background: var(--igds-bg-warning-with-border); color: var(--igds-text-warning); border: 1px solid var(--igds-border-warning); }
    .igds-toast--failure { background: var(--igds-bg-failure-with-border); color: var(--igds-text-failure); border: 1px solid var(--igds-border-failure); }
    .igds-toast--info { background: var(--igds-bg-secondary-pressed); color: var(--igds-text-link-default); border: 1px solid var(--igds-border-brand-default); }
    .igds-toast__icon { font-size: var(--igds-font-size-lg); flex-shrink: 0; }
    .igds-toast__message { flex: 1; }
    .igds-toast__close {
      display: flex; background: none; border: none; cursor: pointer; padding: var(--igds-space-4);
      color: inherit; border-radius: var(--igds-radius-full);
    }
    .igds-toast__close:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
  `]
})
export class IgdsToastComponent implements OnChanges, OnDestroy {
  @Input() message = '';
  @Input() type: 'success' | 'warning' | 'failure' | 'info' = 'info';
  @Input() visible = false;
  @Input() duration = 5000;
  @Output() closed = new EventEmitter<void>();

  private timer: any;

  ngOnChanges(changes: SimpleChanges) {
    if (changes['visible'] && this.visible && this.duration > 0) {
      this.startTimer();
    }
  }

  ngOnDestroy() { clearTimeout(this.timer); }

  close() {
    this.visible = false;
    clearTimeout(this.timer);
    this.closed.emit();
  }

  private startTimer() {
    clearTimeout(this.timer);
    this.timer = setTimeout(() => this.close(), this.duration);
  }
}
