import { Component, Input, Output, EventEmitter, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'igds-input-field',
  template: `
    <div class="igds-input" [class.igds-input--error]="error" [class.igds-input--disabled]="disabled">
      <label *ngIf="label" class="igds-input__label" [for]="inputId">
        {{label}} <span *ngIf="required" class="igds-input__required">*</span>
      </label>
      <div class="igds-input__wrapper" [class.igds-input__wrapper--focused]="focused">
        <input
          [id]="inputId"
          [type]="type"
          [placeholder]="placeholder"
          [disabled]="disabled"
          [readonly]="readonly"
          [attr.aria-describedby]="error ? inputId + '-error' : helperText ? inputId + '-helper' : null"
          [attr.aria-invalid]="error ? true : null"
          [value]="value"
          (input)="onInput($event)"
          (focus)="focused = true"
          (blur)="onBlur()"
          class="igds-input__field" />
      </div>
      <span *ngIf="error" [id]="inputId + '-error'" class="igds-input__error" role="alert">{{error}}</span>
      <span *ngIf="helperText && !error" [id]="inputId + '-helper'" class="igds-input__helper">{{helperText}}</span>
    </div>
  `,
  styles: [`
    :host { display: block; }
    .igds-input__label {
      display: block; font-family: var(--igds-font-family); font-size: var(--igds-font-size-sm);
      font-weight: var(--igds-font-weight-medium); color: var(--igds-text-primary);
      margin-bottom: var(--igds-space-4);
    }
    .igds-input__required { color: var(--igds-text-failure); }
    .igds-input__wrapper {
      border: 1px solid var(--igds-border-subtle-default); border-radius: var(--igds-radius-md);
      transition: border-color var(--igds-transition-fast); background: var(--igds-bg-neutral);
    }
    .igds-input__wrapper:hover { border-color: var(--igds-border-subtle-hover); }
    .igds-input__wrapper--focused { border-color: var(--igds-border-active); border-width: 2px; }
    .igds-input--error .igds-input__wrapper { border-color: var(--igds-border-failure); }
    .igds-input--disabled .igds-input__wrapper {
      background: var(--igds-bg-disabled-with-border); border-color: var(--igds-border-disabled);
    }
    .igds-input__field {
      width: 100%; padding: var(--igds-space-8) var(--igds-space-12); border: none; outline: none;
      font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary); background: transparent; direction: inherit;
    }
    .igds-input__field::placeholder { color: var(--igds-text-secondary); }
    .igds-input__field:disabled { color: var(--igds-text-disabled); cursor: not-allowed; }
    .igds-input__error {
      display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-failure);
      margin-top: var(--igds-space-4);
    }
    .igds-input__helper {
      display: block; font-size: var(--igds-font-size-xs); color: var(--igds-text-secondary);
      margin-top: var(--igds-space-4);
    }
  `],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IgdsInputFieldComponent), multi: true }]
})
export class IgdsInputFieldComponent implements ControlValueAccessor {
  @Input() label = '';
  @Input() placeholder = '';
  @Input() type = 'text';
  @Input() error = '';
  @Input() helperText = '';
  @Input() required = false;
  @Input() disabled = false;
  @Input() readonly = false;
  @Input() inputId = 'igds-input-' + Math.random().toString(36).substr(2, 9);

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
