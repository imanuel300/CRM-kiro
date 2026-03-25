import { Component, Input } from '@angular/core';

@Component({
  selector: 'igds-card',
  template: `
    <div class="igds-card" [class.igds-card--elevated]="elevated">
      <div class="igds-card__header"><ng-content select="[igds-card-header]"></ng-content></div>
      <div class="igds-card__body"><ng-content></ng-content></div>
      <div class="igds-card__footer"><ng-content select="[igds-card-footer]"></ng-content></div>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-card {
      background: var(--igds-bg-neutral); border: 1px solid var(--igds-border-divider);
      border-radius: var(--igds-radius-lg); font-family: var(--igds-font-family);
      overflow: hidden; transition: box-shadow var(--igds-transition-fast);
    }
    .igds-card--elevated { box-shadow: var(--igds-shadow-md); border: none; }
    .igds-card__header {
      padding: var(--igds-space-16) var(--igds-space-24);
      border-bottom: 1px solid var(--igds-border-divider);
    }
    .igds-card__header:empty { display: none; }
    .igds-card__body { padding: var(--igds-space-24); color: var(--igds-text-primary); }
    .igds-card__footer {
      padding: var(--igds-space-16) var(--igds-space-24);
      border-top: 1px solid var(--igds-border-divider);
    }
    .igds-card__footer:empty { display: none; }
  `]
})
export class IgdsCardComponent {
  @Input() elevated = false;
}
