import { Component, Input } from '@angular/core';

@Component({
  selector: 'app-loading-spinner',
  template: `
    <div class="igds-spinner-container" *ngIf="loading">
      <div class="igds-spinner" [style.width.px]="diameter" [style.height.px]="diameter"
        role="progressbar" aria-label="טוען...">
      </div>
    </div>
  `,
  styles: [
    `
      :host {
        display: block;
        direction: inherit;
      }
      .igds-spinner-container {
        display: flex;
        justify-content: center;
        align-items: center;
        padding: var(--igds-space-24);
      }
      .igds-spinner {
        border-radius: var(--igds-radius-full);
        border: 3px solid var(--igds-bg-neutral-secondlevel);
        border-top-color: var(--igds-bg-brand-default);
        animation: igds-spin 0.8s linear infinite;
      }
      @keyframes igds-spin {
        to {
          transform: rotate(360deg);
        }
      }
    `,
  ],
})
export class LoadingSpinnerComponent {
  @Input() loading = false;
  @Input() diameter = 40;
}
