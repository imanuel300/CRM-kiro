import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'igds-date-picker',
  template: `
    <div class="igds-datepicker" [class.igds-datepicker--error]="error" [class.igds-datepicker--disabled]="disabled">
      <label *ngIf="label" class="igds-datepicker__label" [for]="inputId">
        {{label}} <span *ngIf="required" class="igds-datepicker__required">*</span>
      </label>
      <div class="igds-datepicker__wrapper" [class.igds-datepicker__wrapper--focused]="focused">
        <input
          [id]="inputId" type="date" class="igds-datepicker__input"
          [placeholder]="placeholder" [disabled]="disabled"
          [value]="value"
          [attr.aria-describedby]="error ? inputId + '-error' : null"
          [attr.aria-invalid]="error ? true : null"
          (input)="onInput($event)"
          (focus)="focused = true"
          (blur)="onBlur()" />
      </div>
      <span *ngIf="error" [id]="inputId + '-error'" class="igds-datepicker__error" role="alert">{{error}}</span>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .igds-datepicker { font-family: var(--igds-font-family); }
    .igds-datepicker__label {
      display: block; font-size: var(--igds-font-size-sm); font-weight: var(--igds-font-weight-medium);
      color: var(--igds-text-primary); margin-bottom: var(--igds-space-4);
    }
    .igds-datepicker__required { color: var(--igds-text-failure); }
    .igds-datepicker__wrapper {
      border: 1px solid var(--igds-border-subtle-default); border-radius: var(--igds-radius-md);
      transition: border-color var(--igds-transition-fast); background: var(--igds-bg-neutral);
    }
    .igds-datepicker__wrapper:hover { border-color: var(--igds-border-subtle-hover); }
    .igds-datepicker__wrapper--focused { border-color: var(--igds-border-active); border-width: 2px; }
    .igds-datepicker--error .igds-datepicker__wrapper { border-color: var(--igds-border-failure); }
    .igds-datepicker--disabled .igds-datepicker__wrapper {
      background: var(--igds-bg-disabled-with-border); border-color: var(--igds-border-disabled);
    }
    .igds-datepicker__input {
      width: 100%; padding: var(--igds-space-8) var(--igds-space-12); border: none; outline: none;
      font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary); background: transparent; direction: inherit;
      min-height: 44px; box-sizing: border-box;
    }
    .igds-datepicker__input:disabled { color: var(--igds-text-disabled); cursor: not-allowed; }
    .igds-datepicker__error {
      display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-failure);
      margin-top: var(--igds-space-4);
    }
  `],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IgdsDatePickerComponent), multi: true }]
})
export class IgdsDatePickerComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() placeholder = '';
  @Input() error = '';
  @Input() required = false;
  @Input() disabled = false;
  @Input() inputId = 'igds-datepicker-' + Math.random().toString(36).substr(2, 9);

  value = '';
  focused = false;
  private onChange: (val: string) => void = () => {};
  private onTouched: () => void = () => {};

  onInput(event: Event) {
    this.value = (event.target as HTMLInputElement).value;
    this.onChange(this.value);
  }
  onBlur() { this.focused = false; this.onTouched(); }
  writeValue(val: string) { this.value = val || ''; }
  registerOnChange(fn: (val: string) => void) { this.onChange = fn; }
  registerOnTouched(fn: () => void) { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean) { this.disabled = isDisabled; }
}
