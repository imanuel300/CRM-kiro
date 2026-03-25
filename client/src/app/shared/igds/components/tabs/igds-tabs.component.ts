import { Component, Input, Output, EventEmitter } from '@angular/core';

export interface IgdsTab {
  label: string;
  id: string;
}

@Component({
  selector: 'igds-tabs',
  template: `
    <div class="igds-tabs" role="tablist" aria-orientation="horizontal">
      <button *ngFor="let tab of tabs; let i = index"
        class="igds-tabs__tab"
        [class.igds-tabs__tab--active]="activeTab === tab.id"
        role="tab"
        [id]="'tab-' + tab.id"
        [attr.aria-selected]="activeTab === tab.id"
        [attr.aria-controls]="'panel-' + tab.id"
        [tabindex]="activeTab === tab.id ? 0 : -1"
        (click)="selectTab(tab)"
        (keydown)="onKeydown($event, i)">
        {{tab.label}}
      </button>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-tabs {
      display: flex; gap: 0; border-bottom: 2px solid var(--igds-border-divider);
      font-family: var(--igds-font-family); overflow-x: auto;
    }
    .igds-tabs__tab {
      padding: var(--igds-space-8) var(--igds-space-16); background: none; border: none;
      border-bottom: 2px solid transparent; margin-bottom: -2px; cursor: pointer;
      font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
      font-weight: var(--igds-font-weight-medium); color: var(--igds-text-secondary);
      transition: all var(--igds-transition-fast); white-space: nowrap; min-height: 44px;
    }
    .igds-tabs__tab:hover { color: var(--igds-text-primary); }
    .igds-tabs__tab--active { color: var(--igds-text-link-default); border-bottom-color: var(--igds-border-brand-default); }
    .igds-tabs__tab:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: -2px; }
  `]
})
export class IgdsTabsComponent {
  @Input() tabs: IgdsTab[] = [];
  @Input() activeTab = '';
  @Output() tabChange = new EventEmitter<string>();

  selectTab(tab: IgdsTab) {
    this.activeTab = tab.id;
    this.tabChange.emit(tab.id);
  }

  onKeydown(event: KeyboardEvent, index: number) {
    let newIndex = index;
    if (event.key === 'ArrowRight' || event.key === 'ArrowLeft') {
      event.preventDefault();
      newIndex = event.key === 'ArrowRight' ? (index + 1) % this.tabs.length : (index - 1 + this.tabs.length) % this.tabs.length;
      this.selectTab(this.tabs[newIndex]);
    }
  }
}
