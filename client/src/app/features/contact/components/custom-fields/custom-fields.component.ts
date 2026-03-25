import { Component, Input, OnChanges, SimpleChanges } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { CustomFieldValue } from '../../models/contact.models';
import { ContactService } from '../../services/contact.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-custom-fields',
  template: `
    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div *ngIf="!loading && fields.length === 0" class="no-data">
      אין שדות מותאמים אישית מוגדרים ליחידה זו
    </div>

    <form [formGroup]="form" *ngIf="!loading && fields.length > 0" (ngSubmit)="onSave()">
      <div *ngFor="let field of fields" class="field-row">
        <igds-input-field
          *ngIf="field.fieldType === 'text'"
          [label]="field.fieldName"
          [formControlName]="'field_' + field.customFieldDefinitionId">
        </igds-input-field>

        <igds-input-field
          *ngIf="field.fieldType === 'number'"
          [label]="field.fieldName"
          type="number"
          [formControlName]="'field_' + field.customFieldDefinitionId">
        </igds-input-field>

        <igds-date-picker
          *ngIf="field.fieldType === 'date'"
          [label]="field.fieldName"
          [formControlName]="'field_' + field.customFieldDefinitionId">
        </igds-date-picker>

        <igds-dropdown
          *ngIf="field.fieldType === 'boolean'"
          [label]="field.fieldName"
          [formControlName]="'field_' + field.customFieldDefinitionId"
          [options]="booleanOptions">
        </igds-dropdown>
      </div>

      <div class="form-actions">
        <igds-button variant="primary" type="submit" [disabled]="saving">
          {{ saving ? 'שומר...' : 'שמירה' }}
        </igds-button>
      </div>
    </form>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .field-row {
      margin-block-end: var(--igds-space-8);
    }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
    .no-data {
      text-align: center;
      padding: var(--igds-space-24);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
  `],
})
export class CustomFieldsComponent implements OnChanges {
  @Input() contactId!: number;
  @Input() orgUnitId!: number;

  form!: FormGroup;
  fields: CustomFieldValue[] = [];
  loading = false;
  saving = false;

  booleanOptions: IgdsDropdownOption[] = [
    { value: 'true', label: 'כן' },
    { value: 'false', label: 'לא' },
  ];

  constructor(
    private fb: FormBuilder,
    private contactService: ContactService,
    private notification: NotificationService
  ) {
    this.form = this.fb.group({});
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ((changes['contactId'] || changes['orgUnitId']) && this.contactId && this.orgUnitId) {
      this.loadFields();
    }
  }

  private loadFields(): void {
    this.loading = true;
    this.contactService.getCustomFields(this.contactId, this.orgUnitId).subscribe({
      next: (fields: CustomFieldValue[]) => {
        this.fields = fields;
        this.buildForm(fields);
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת שדות מותאמים');
        this.loading = false;
      },
    });
  }

  private buildForm(fields: CustomFieldValue[]): void {
    const group: Record<string, any> = {};
    fields.forEach((f) => {
      group['field_' + f.customFieldDefinitionId] = [f.value || ''];
    });
    this.form = this.fb.group(group);
  }

  onSave(): void {
    this.saving = true;
    let pending = this.fields.length;
    let hasError = false;

    if (pending === 0) {
      this.saving = false;
      return;
    }

    this.fields.forEach((field) => {
      const value = this.form.get('field_' + field.customFieldDefinitionId)?.value;
      this.contactService
        .setCustomFieldValue({
          contactId: this.contactId,
          orgUnitId: this.orgUnitId,
          customFieldDefinitionId: field.customFieldDefinitionId,
          value: value || undefined,
        })
        .subscribe({
          next: () => {
            pending--;
            if (pending === 0 && !hasError) {
              this.notification.success('השדות נשמרו בהצלחה');
              this.saving = false;
            }
          },
          error: () => {
            hasError = true;
            pending--;
            if (pending === 0) {
              this.notification.error('שגיאה בשמירת חלק מהשדות');
              this.saving = false;
            }
          },
        });
    });
  }
}
