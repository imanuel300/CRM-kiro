import { Component, Input, Output, EventEmitter } from '@angular/core';

export interface IgdsBreadcrumbItem {
  label: string;
  url?: string;
}

@Component({
  selector: 'igds-breadcrumbs',
  template: `
    <nav class="igds-breadcrumbs" aria-label="ניווט פירורי לחם">
      <ol class="igds-breadcrumbs__list">
        <li *ngFor="let item of items; let last = last" class="igds-breadcrumbs__item">
          <a *ngIf="!last && item.url" class="igds-breadcrumbs__link"
            [href]="item.url" (click)="onNavigate($event, item)">{{item.label}}</a>
          <span *ngIf="last || !item.url" class="igds-breadcrumbs__current"
            [attr.aria-current]="last ? 'page' : null">{{item.label}}</span>
          <span *ngIf="!last" class="igds-breadcrumbs__sep" aria-hidden="true">/</span>
        </li>
      </ol>
    </nav>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-breadcrumbs { font-family: var(--igds-font-family); }
    .igds-breadcrumbs__list {
      display: flex; align-items: center; gap: var(--igds-space-4);
      list-style: none; margin: 0; padding: 0; flex-wrap: wrap;
    }
    .igds-breadcrumbs__item { display: flex; align-items: center; gap: var(--igds-space-4); }
    .igds-breadcrumbs__link {
      color: var(--igds-text-link-default); text-decoration: none;
      font-size: var(--igds-font-size-sm); transition: color var(--igds-transition-fast);
    }
    .igds-breadcrumbs__link:hover { color: var(--igds-text-link-hover); text-decoration: underline; }
    .igds-breadcrumbs__link:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; border-radius: var(--igds-radius-sm); }
    .igds-breadcrumbs__current { color: var(--igds-text-secondary); font-size: var(--igds-font-size-sm); }
    .igds-breadcrumbs__sep { color: var(--igds-text-secondary); font-size: var(--igds-font-size-xs); }
  `]
})
export class IgdsBreadcrumbsComponent {
  @Input() items: IgdsBreadcrumbItem[] = [];
  @Output() navigate = new EventEmitter<IgdsBreadcrumbItem>();

  onNavigate(event: Event, item: IgdsBreadcrumbItem) {
    event.preventDefault();
    this.navigate.emit(item);
  }
}
