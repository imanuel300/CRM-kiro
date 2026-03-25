import { Component, Input, forwardRef, HostListener, ElementRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface IgdsDropdownOption {
  value: any;
  label: string;
}

@Component({
  selector: 'igds-dropdown',
  template: `
    <div class="igds-dropdown" [class.igds-dropdown--error]="error" [class.igds-dropdown--disabled]="disabled" [class.igds-dropdown--open]="open">
      <label *ngIf="label" class="igds-dropdown__label" [for]="dropdownId">
        {{label}} <span *ngIf="required" class="igds-dropdown__required">*</span>
      </label>
      <div class="igds-dropdown__trigger"
        [id]="dropdownId"
        role="combobox"
        tabindex="0"
        [attr.aria-expanded]="open"
        [attr.aria-haspopup]="'listbox'"
        [attr.aria-describedby]="error ? dropdownId + '-error' : null"
        [attr.aria-invalid]="error ? true : null"
        [attr.aria-disabled]="disabled"
        (click)="toggle()"
        (keydown)="onKeydown($event)">
        <span class="igds-dropdown__value" [class.igds-dropdown__placeholder]="!selectedLabel">
          {{selectedLabel || placeholder}}
        </span>
        <span class="igds-dropdown__arrow">&#9662;</span>
      </div>
      <ul *ngIf="open" class="igds-dropdown__list" role="listbox" [attr.aria-labelledby]="dropdownId">
        <li *ngFor="let opt of options; let i = index"
          class="igds-dropdown__option"
          role="option"
          [attr.aria-selected]="value === opt.value"
          [class.igds-dropdown__option--selected]="value === opt.value"
          [class.igds-dropdown__option--focused]="focusedIndex === i"
          (click)="select(opt)"
          (mouseenter)="focusedIndex = i">
          {{opt.label}}
        </li>
      </ul>
      <span *ngIf="error" [id]="dropdownId + '-error'" class="igds-dropdown__error" role="alert">{{error}}</span>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-dropdown { position: relative; font-family: var(--igds-font-family); }
    .igds-dropdown__label {
      display: block; font-size: var(--igds-font-size-sm); font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary); margin-bottom: var(--igds-space-4);
    }
    .igds-dropdown__required { color: var(--igds-text-failure); }
    .igds-dropdown__trigger {
      display: flex; align-items: center; justify-content: space-between;
      padding: var(--igds-space-8) var(--igds-space-12); border: 1px solid var(--igds-border-subtle-default);
      border-radius: var(--igds-radius-md); background: var(--igds-bg-neutral);
      cursor: pointer; min-height: 44px; transition: border-color var(--igds-transition-fast);
      font-size: var(--igds-font-size-md); color: var(--igds-text-primary);
    }
    .igds-dropdown__trigger:hover { border-color: var(--igds-border-subtle-hover); }
    .igds-dropdown__trigger:focus-visible { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    .igds-dropdown--open .igds-dropdown__trigger { border-color: var(--igds-border-active); border-width: 2px; }
    .igds-dropdown--error .igds-dropdown__trigger { border-color: var(--igds-border-failure); }
    .igds-dropdown--disabled .igds-dropdown__trigger {
      background: var(--igds-bg-disabled-with-border); border-color: var(--igds-border-disabled);
      color: var(--igds-text-disabled); cursor: not-allowed;
    }
    .igds-dropdown__placeholder { color: var(--igds-text-secondary); }
    .igds-dropdown__arrow { font-size: var(--igds-font-size-xs); color: var(--igds-text-secondary); }
    .igds-dropdown__list {
      position: absolute; top: 100%; left: 0; right: 0; z-index: 100;
      margin: var(--igds-space-4) 0 0; padding: var(--igds-space-4) 0; list-style: none;
      background: var(--igds-bg-neutral); border: 1px solid var(--igds-border-subtle-default);
      border-radius: var(--igds-radius-md); box-shadow: var(--igds-shadow-md); max-height: 240px; overflow-y: auto;
    }
    .igds-dropdown__option {
      padding: var(--igds-space-8) var(--igds-space-12); cursor: pointer;
      font-size: var(--igds-font-size-md); color: var(--igds-text-primary);
      transition: background var(--igds-transition-fast);
    }
    .igds-dropdown__option:hover, .igds-dropdown__option--focused { background: var(--igds-bg-neutral-hover); }
    .igds-dropdown__option--selected { font-weight: var(--igds-font-weight-medium); color: var(--igds-text-link-default); }
    .igds-dropdown__error {
      display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-failure);
      margin-top: var(--igds-space-4);
    }
  `],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IgdsDropdownComponent), multi: true }]
})
export class IgdsDropdownComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() placeholder = 'בחר...';
  @Input() options: IgdsDropdownOption[] = [];
  @Input() error = '';
  @Input() required = false;
  @Input() disabled = false;
  @Input() dropdownId = 'igds-dropdown-' + Math.random().toString(36).substr(2, 9);

  value: any = null;
  open = false;
  focusedIndex = -1;
  private onChange: (val: any) => void = () => {};
  private onTouched: () => void = () => {};

  constructor(private elRef: ElementRef) {}

  get selectedLabel(): string {
    const found = this.options.find(o => o.value === this.value);
    return found ? found.label : '';
  }

  @HostListener('document:click', ['$event'])
  onDocClick(event: Event) {
    if (!this.elRef.nativeElement.contains(event.target)) { this.open = false; }
  }

  toggle() {
    if (this.disabled) return;
    this.open = !this.open;
    if (this.open) { this.focusedIndex = this.options.findIndex(o => o.value === this.value); }
  }

  select(opt: IgdsDropdownOption) {
    this.value = opt.value;
    this.onChange(this.value);
    this.open = false;
  }

  onKeydown(event: KeyboardEvent) {
    if (this.disabled) return;
    switch (event.key) {
      case 'Enter': case ' ':
        event.preventDefault();
        if (this.open && this.focusedIndex >= 0) { this.select(this.options[this.focusedIndex]); }
        else { this.toggle(); }
        break;
      case 'ArrowDown':
        event.preventDefault();
        if (!this.open) { this.open = true; }
        this.focusedIndex = Math.min(this.focusedIndex + 1, this.options.length - 1);
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.focusedIndex = Math.max(this.focusedIndex - 1, 0);
        break;
      case 'Escape':
        this.open = false;
        break;
    }
  }

  writeValue(val: any) { this.value = val; }
  registerOnChange(fn: (val: any) => void) { this.onChange = fn; }
  registerOnTouched(fn: () => void) { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean) { this.disabled = isDisabled; }
}
