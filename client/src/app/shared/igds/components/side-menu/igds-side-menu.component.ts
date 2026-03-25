import { Component, Input, Output, EventEmitter } from '@angular/core';

export interface IgdsSideMenuItem {
  label: string;
  icon?: string;
  route?: string;
  children?: IgdsSideMenuItem[];
}

@Component({
  selector: 'igds-side-menu',
  template: `
    <nav class="igds-sidemenu" [class.igds-sidemenu--collapsed]="collapsed" role="navigation" aria-label="תפריט צד">
      <ul class="igds-sidemenu__list">
        <li *ngFor="let item of items" class="igds-sidemenu__item">
          <button class="igds-sidemenu__btn" type="button"
            [attr.aria-expanded]="item.children?.length ? isExpanded(item) : null"
            [title]="collapsed ? item.label : ''"
            (click)="onItemClick(item)">
            <span *ngIf="item.icon" class="igds-sidemenu__icon">{{item.icon}}</span>
            <span *ngIf="!collapsed" class="igds-sidemenu__label">{{item.label}}</span>
            <span *ngIf="!collapsed && item.children?.length" class="igds-sidemenu__arrow"
              [class.igds-sidemenu__arrow--open]="isExpanded(item)">&#9662;</span>
          </button>
          <ul *ngIf="!collapsed && item.children?.length && isExpanded(item)" class="igds-sidemenu__sublist">
            <li *ngFor="let child of item.children" class="igds-sidemenu__subitem">
              <button class="igds-sidemenu__btn igds-sidemenu__btn--sub" type="button"
                (click)="onItemClick(child)">
                <span class="igds-sidemenu__label">{{child.label}}</span>
              </button>
            </li>
          </ul>
        </li>
      </ul>
    </nav>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-sidemenu { font-family: var(--igds-font-family); background: var(--igds-bg-neutral); min-height: 100%; }
    .igds-sidemenu--collapsed { width: 56px; }
    .igds-sidemenu__list { list-style: none; margin: 0; padding: var(--igds-space-8) 0; }
    .igds-sidemenu__btn {
      display: flex; align-items: center; gap: var(--igds-space-8); width: 100%;
      padding: var(--igds-space-8) var(--igds-space-16); background: none; border: none;
      cursor: pointer; font-family: var(--igds-font-family); font-size: var(--igds-font-size-sm);
      color: var(--igds-text-primary); transition: background var(--igds-transition-fast);
      min-height: 40px; text-align: inherit;
    }
    .igds-sidemenu__btn:hover { background: var(--igds-bg-neutral-hover); }
    .igds-sidemenu__btn:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: -2px; }
    .igds-sidemenu__btn--sub { padding-inline-start: var(--igds-space-40); font-size: var(--igds-font-size-xs); }
    .igds-sidemenu__icon { flex-shrink: 0; width: 20px; text-align: center; }
    .igds-sidemenu__label { flex: 1; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }
    .igds-sidemenu__arrow {
      font-size: var(--igds-font-size-xs); transition: transform var(--igds-transition-fast);
    }
    .igds-sidemenu__arrow--open { transform: rotate(180deg); }
    .igds-sidemenu__sublist { list-style: none; margin: 0; padding: 0; }
  `]
})
export class IgdsSideMenuComponent {
  @Input() items: IgdsSideMenuItem[] = [];
  @Input() collapsed = false;
  @Output() itemClick = new EventEmitter<IgdsSideMenuItem>();

  private expandedItems = new Set<IgdsSideMenuItem>();

  isExpanded(item: IgdsSideMenuItem): boolean { return this.expandedItems.has(item); }

  onItemClick(item: IgdsSideMenuItem) {
    if (item.children?.length) {
      if (this.expandedItems.has(item)) { this.expandedItems.delete(item); }
      else { this.expandedItems.add(item); }
    }
    this.itemClick.emit(item);
  }
}
