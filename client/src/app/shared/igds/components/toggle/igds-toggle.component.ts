import { Component, Input, forwardRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

@Component({
  selector: 'igds-toggle',
  template: `
    <label class="igds-toggle" [class.igds-toggle--disabled]="disabled" [class.igds-toggle--checked]="checked">
      <input type="checkbox" class="igds-toggle__input" role="switch"
        [checked]="checked" [disabled]="disabled"
        [attr.aria-checked]="checked"
        (change)="onToggle($event)" />
      <span class="igds-toggle__track">
        <span class="igds-toggle__thumb"></span>
      </span>
      <span class="igds-toggle__label" *ngIf="label">{{label}}</span>
    </label>
  `,
  styles: [`
    :host { display: inline-block; direction: inherit; }
    .igds-toggle {
      display: inline-flex; align-items: center; gap: var(--igds-space-8);
      cursor: pointer; font-family: var(--igds-font-family); font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary); min-height: 44px;
    }
    .igds-toggle--disabled { cursor: not-allowed; color: var(--igds-text-disabled); }
    .igds-toggle__input { position: absolute; opacity: 0; width: 0; height: 0; }
    .igds-toggle__track {
      position: relative; width: 44px; height: 24px;
      background: var(--igds-border-subtle-default); border-radius: var(--igds-radius-full);
      transition: background var(--igds-transition-fast); flex-shrink: 0;
    }
    .igds-toggle__thumb {
      position: absolute; top: 2px; inset-inline-start: 2px;
      width: 20px; height: 20px; border-radius: var(--igds-radius-full);
      background: var(--igds-bg-neutral); box-shadow: var(--igds-shadow-sm);
      transition: inset-inline-start var(--igds-transition-fast);
    }
    .igds-toggle--checked .igds-toggle__track { background: var(--igds-bg-brand-default); }
    .igds-toggle--checked .igds-toggle__thumb { inset-inline-start: 22px; }
    .igds-toggle--disabled .igds-toggle__track { background: var(--igds-bg-disabled-without-border); }
    .igds-toggle__input:focus-visible + .igds-toggle__track { outline: 2px solid var(--igds-border-focused); outline-offset: 2px; }
    .igds-toggle__label { user-select: none; }
  `],
  providers: [{ provide: NG_VALUE_ACCESSOR, useExisting: forwardRef(() => IgdsToggleComponent), multi: true }]
})
export class IgdsToggleComponent implements ControlValueAccessor {
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
