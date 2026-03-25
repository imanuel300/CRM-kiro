import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'igds-checkbox',
  template: `
    <label class="igds-checkbox" [class.igds-checkbox--disabled]="disabled" [class.igds-checkbox--checked]="checked">
      <input type="checkbox" class="igds-checkbox__input"
        [checked]="checked" [disabled]="disabled"
        [attr.aria-checked]="checked"
        (change)="onToggle($event)" />
      <span class="igds-checkbox__box">
        <svg *ngIf="checked" width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="3" stroke-linecap="round" stroke-linejoin="round">
          <polyline points="20 6 9 17 4 12"/>
        </svg>
      </span>
      <span class="igds-checkbox__label" *ngIf="label">{{label}}</span>
    </label>
  `,
  styles: [`
    :host { display: inline-block; direction: inherit; }
    .igds-checkbox {
      display: inline-flex; align-items: center; gap: var(--igds-space-8);
      cursor: pointer; font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary); min-height: 44px;
    }
    .igds-checkbox--disabled { cursor: not-allowed; color: var(--igds-text-disabled); }
    .igds-checkbox__input { position: absolute; opacity: 0; width: 0; height: 0; }
    .igds-checkbox__box {
      display: flex; align-items: center; justify-content: center;
      width: 20px; height: 20px; border: 2px solid var(--igds-border-subtle-default);
      border-radius: var(--igds-radius-sm); background: var(--igds-bg-neutral);
      transition: all var(--igds-transition-fast); flex-shrink: 0;
    }
    .igds-checkbox:hover:not(.igds-checkbox--disabled) .igds-checkbox__box { border-color: var(--igds-border-subtle-hover); }
    .igds-checkbox--checked .igds-checkbox__box {
      background: var(--igds-bg-brand-default); border-color: var(--igds-bg-brand-default); color: var(--igds-text-inverted);
    }
    .igds-checkbox--disabled .igds-checkbox__box {
      background: var(--igds-bg-disabled-with-border); border-color: var(--igds-border-disabled);
    }
    .igds-checkbox__input:focus-visible + .igds-checkbox__box { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    .igds-checkbox__label { user-select: none; }
  `],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IgdsCheckboxComponent), multi: true }]
})
export class IgdsCheckboxComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() disabled = false;
  @Input() checked = false;

  private onChange: (val: boolean) => void = () => {};
  private onTouched: () => void = () => {};

  onToggle(event: Event) {
    if (this.disabled) return;
    this.checked = (event.target as HTMLInputElement).checked;
    this.onChange(this.checked);
    this.onTouched();
  }

  writeValue(val: boolean) { this.checked = !!val; }
  registerOnChange(fn: (val: boolean) => void) { this.onChange = fn; }
  registerOnTouched(fn: () => void) { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean) { this.disabled = isDisabled; }
}
