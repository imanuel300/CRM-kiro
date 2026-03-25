import { Component, Input, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'igds-search-field',
  template: `
    <div class="igds-search" [class.igds-search--focused]="focused">
      <span class="igds-search__icon" aria-hidden="true">
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
        </svg>
      </span>
      <input
        class="igds-search__input"
        type="search"
        [placeholder]="placeholder"
        [value]="value"
        [attr.aria-label]="placeholder || 'חיפוש'"
        (input)="onInput($event)"
        (keydown.enter)="onSearch()"
        (focus)="focused = true"
        (blur)="focused = false" />
      <button *ngIf="value" class="igds-search__clear" type="button"
        aria-label="נקה חיפוש" (click)="onClear()">
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
          <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
        </svg>
      </button>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-search {
      display: flex; align-items: center; gap: var(--igds-space-8);
      padding: var(--igds-space-8) var(--igds-space-12);
      border: 1px solid var(--igds-border-subtle-default); border-radius: var(--igds-radius-md);
      background: var(--igds-bg-neutral); transition: border-color var(--igds-transition-fast);
      font-family: var(--igds-font-family); min-height: 44px;
    }
    .igds-search:hover { border-color: var(--igds-border-subtle-hover); }
    .igds-search--focused { border-color: var(--igds-border-active); border-width: 2px; }
    .igds-search__icon { display: flex; color: var(--igds-text-secondary); flex-shrink: 0; }
    .igds-search__input {
      flex: 1; border: none; outline: none; background: transparent;
      font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary); direction: inherit; min-width: 0;
    }
    .igds-search__input::placeholder { color: var(--igds-text-secondary); }
    .igds-search__input::-webkit-search-cancel-button { display: none; }
    .igds-search__clear {
      display: flex; align-items: center; justify-content: center;
      background: none; border: none; cursor: pointer; padding: var(--igds-space-4);
      color: var(--igds-text-secondary); border-radius: var(--igds-radius-full);
      transition: color var(--igds-transition-fast);
    }
    .igds-search__clear:hover { color: var(--igds-text-primary); }
    .igds-search__clear:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
  `]
})
export class IgdsSearchFieldComponent {
  @Input() placeholder = 'חיפוש...';
  @Input() value = '';
  @Output() search = new EventEmitter<string>();
  @Output() clear = new EventEmitter<void>();

  focused = false;

  onInput(event: Event) {
    this.value = (event.target as HTMLInputElement).value;
    this.search.emit(this.value);
  }

  onSearch() { this.search.emit(this.value); }

  onClear() {
    this.value = '';
    this.clear.emit();
    this.search.emit('');
  }
}
