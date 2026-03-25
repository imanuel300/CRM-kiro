import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

export interface IgdsRadioOption {
  value: any;
  label: string;
}

@Component({
  selector: 'igds-radio-button',
  template: `
    <label class="igds-radio" [class.igds-radio--disabled]="disabled" [class.igds-radio--checked]="checked">
      <input type="radio" class="igds-radio__input"
        [name]="name" [value]="value" [checked]="checked" [disabled]="disabled"
        [attr.aria-checked]="checked"
        (change)="onSelect()" />
      <span class="igds-radio__circle">
        <span *ngIf="checked" class="igds-radio__dot"></span>
      </span>
      <span class="igds-radio__label" *ngIf="label">{{label}}</span>
    </label>
  `,
  styles: [`
    :host { display: inline-block; direction: inherit; }
    .igds-radio {
      display: inline-flex; align-items: center; gap: var(--igds-space-8);
      cursor: pointer; font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary); min-height: 44px;
    }
    .igds-radio--disabled { cursor: not-allowed; color: var(--igds-text-disabled); }
    .igds-radio__input { position: absolute; opacity: 0; width: 0; height: 0; }
    .igds-radio__circle {
      display: flex; align-items: center; justify-content: center;
      width: 20px; height: 20px; border: 2px solid var(--igds-border-subtle-default);
      border-radius: var(--igds-radius-full); background: var(--igds-bg-neutral);
      transition: all var(--igds-transition-fast); flex-shrink: 0;
    }
    .igds-radio:hover:not(.igds-radio--disabled) .igds-radio__circle { border-color: var(--igds-border-subtle-hover); }
    .igds-radio--checked .igds-radio__circle { border-color: var(--igds-bg-brand-default); }
    .igds-radio__dot {
      width: 10px; height: 10px; border-radius: var(--igds-radius-full);
      background: var(--igds-bg-brand-default);
    }
    .igds-radio--disabled .igds-radio__circle {
      background: var(--igds-bg-disabled-with-border); border-color: var(--igds-border-disabled);
    }
    .igds-radio--disabled .igds-radio__dot { background: var(--igds-border-disabled); }
    .igds-radio__input:focus-visible + .igds-radio__circle { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    .igds-radio__label { user-select: none; }
  `],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IgdsRadioButtonComponent), multi: true }]
})
export class IgdsRadioButtonComponent implements ControlValueAccessor {
  @Input() options: IgdsRadioOption[] = [];
  @Input() label = '';
  @Input() name = '';
  @Input() value: any = '';
  @Input() disabled = false;
  @Input() checked = false;

  private onChange: (val: any) => void = () => {};
  private onTouched: () => void = () => {};

  onSelect() {
    if (this.disabled) return;
    this.checked = true;
    this.onChange(this.value);
    this.onTouched();
  }

  writeValue(val: any) { this.checked = val === this.value; }
  registerOnChange(fn: (val: any) => void) { this.onChange = fn; }
  registerOnTouched(fn: () => void) { this.onTouched = fn; }
  setDisabledState(isDisabled: boolean) { this.disabled = isDisabled; }
}
