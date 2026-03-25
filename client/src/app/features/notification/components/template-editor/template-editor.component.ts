import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  NotificationTemplate,
  TRIGGER_EVENTS,
  TEMPLATE_VARIABLES,
} from '../../models/notification.models';
import { NotificationApiService } from '../../services/notification-api.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-template-editor',
  template: `
    <div class="page-header">
      <h1>{{ isEdit ? 'עריכת תבנית דיוור' : 'תבנית דיוור חדשה' }}</h1>
    </div>

    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div class="editor-layout" *ngIf="!loading">
      <igds-card class="form-card">
        <h2 igds-card-header>פרטי תבנית</h2>
        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <igds-input-field
            label="שם התבנית"
            formControlName="name"
            [required]="true"
            [error]="form.get('name')?.touched && form.get('name')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="נושא ההודעה"
            formControlName="subject"
            [required]="true"
            [error]="form.get('subject')?.touched && form.get('subject')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <div class="row">
            <igds-dropdown
              label="ערוץ שליחה"
              formControlName="channel"
              [options]="channelOptions"
              [required]="true">
            </igds-dropdown>

            <igds-dropdown
              label="אירוע מפעיל"
              formControlName="triggerEvent"
              [options]="triggerEventOptions"
              [required]="true">
            </igds-dropdown>
          </div>

          <igds-checkbox
            label="תבנית פעילה"
            formControlName="isActive">
          </igds-checkbox>

          <div class="variables-section">
            <h3>משתנים זמינים</h3>
            <div class="variables-list">
              <igds-tag *ngFor="let v of templateVariables"
                [label]="v.label + ' - ' + v.key"
                variant="default"
                (click)="insertVariable(v.key)">
              </igds-tag>
            </div>
          </div>

          <igds-input-field
            label="גוף ההודעה"
            formControlName="body"
            [required]="true"
            [error]="form.get('body')?.touched && form.get('body')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <div class="form-actions">
            <igds-button variant="primary" type="submit"
              [disabled]="form.invalid || saving">
              {{ isEdit ? 'עדכון' : 'יצירה' }}
            </igds-button>
            <igds-button variant="secondary" type="button" (onClick)="onCancel()">
              ביטול
            </igds-button>
          </div>
        </form>
      </igds-card>

      <igds-card class="preview-card">
        <h2 igds-card-header>תצוגה מקדימה</h2>
        <div class="preview-channel">
          <span>{{ form.get('channel')?.value === 'Sms' ? '💬 SMS' : '📧 דוא"ל' }}</span>
        </div>
        <div class="preview-subject" *ngIf="form.get('channel')?.value === 'Email'">
          <strong>נושא:</strong> {{ getPreviewText(form.get('subject')?.value) }}
        </div>
        <div class="preview-body">
          <div [innerHTML]="getPreviewHtml(form.get('body')?.value)"></div>
        </div>
      </igds-card>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .editor-layout {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: var(--igds-space-16);
    }
    .form-card, .preview-card { min-height: 400px; }
    .row {
      display: flex;
      gap: var(--igds-space-16);
    }
    .row > * { flex: 1; }
    .variables-section {
      margin-block-end: var(--igds-space-16);
    }
    .variables-section h3 {
      margin-block-end: var(--igds-space-8);
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-sm);
      color: var(--igds-text-secondary);
    }
    .variables-list {
      display: flex;
      flex-wrap: wrap;
      gap: var(--igds-space-8);
    }
    .variables-list igds-tag { cursor: pointer; }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
    .preview-channel {
      display: flex;
      align-items: center;
      gap: var(--igds-space-8);
      margin-block-end: var(--igds-space-12);
      color: var(--igds-text-secondary);
      font-family: var(--igds-font-family);
    }
    .preview-subject {
      margin-block-end: var(--igds-space-12);
      padding: var(--igds-space-8);
      background: var(--igds-bg-neutral-secondlevel);
      border-radius: var(--igds-radius-md);
      font-family: var(--igds-font-family);
    }
    .preview-body {
      padding: var(--igds-space-16);
      border: 1px solid var(--igds-border-divider);
      border-radius: var(--igds-radius-md);
      min-height: 200px;
      white-space: pre-wrap;
      line-height: 1.6;
      font-family: var(--igds-font-family);
      color: var(--igds-text-primary);
    }
    @media (max-width: 960px) {
      .editor-layout { grid-template-columns: 1fr; }
    }
  `],
})
export class TemplateEditorComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  loading = false;
  saving = false;
  templateId?: number;
  triggerEvents = TRIGGER_EVENTS;
  templateVariables = TEMPLATE_VARIABLES;

  channelOptions: IgdsDropdownOption[] = [
    { value: 'Email', label: 'דוא"ל' },
    { value: 'Sms', label: 'SMS' },
  ];

  triggerEventOptions: IgdsDropdownOption[] = TRIGGER_EVENTS.map(e => ({
    value: e.value,
    label: e.label,
  }));

  private sampleVariables: Record<string, string> = {
    '{{שם_מועמד}}': 'ישראל ישראלי',
    '{{סטטוס}}': 'עבר תנאי סף',
    '{{תאריך}}': new Date().toLocaleDateString('he-IL'),
    '{{קול_קורא}}': 'מכרז עוזרים משפטיים 2024',
    '{{יחידה}}': 'עוזמ"ת',
  };

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private notificationApi: NotificationApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      name: ['', Validators.required],
      subject: ['', Validators.required],
      body: ['', Validators.required],
      channel: ['Email', Validators.required],
      triggerEvent: ['StatusChanged', Validators.required],
      isActive: [true],
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEdit = true;
      this.templateId = +id;
      this.loadTemplate(this.templateId);
    }
  }

  loadTemplate(id: number): void {
    this.loading = true;
    this.notificationApi.getTemplate(id).subscribe({
      next: (template: NotificationTemplate) => {
        this.form.patchValue({
          name: template.name,
          subject: template.subject,
          body: template.body,
          channel: template.channel,
          triggerEvent: template.triggerEvent,
          isActive: template.isActive,
        });
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת התבנית');
        this.loading = false;
      },
    });
  }

  insertVariable(variable: string): void {
    const bodyControl = this.form.get('body');
    const currentValue = bodyControl?.value || '';
    bodyControl?.setValue(currentValue + variable);
  }

  getPreviewText(text: string | null): string {
    if (!text) return '';
    let result = text;
    for (const [key, value] of Object.entries(this.sampleVariables)) {
      result = result.split(key).join(value);
    }
    return result;
  }

  getPreviewHtml(text: string | null): string {
    const preview = this.getPreviewText(text);
    return preview.replace(/\n/g, '<br>');
  }

  onSubmit(): void {
    if (this.form.invalid) return;
    this.saving = true;
    const value = this.form.value;

    if (this.isEdit && this.templateId) {
      this.notificationApi.updateTemplate({ id: this.templateId, ...value }).subscribe({
        next: () => {
          this.notification.success('התבנית עודכנה בהצלחה');
          this.router.navigate(['/notifications/templates']);
        },
        error: () => {
          this.notification.error('שגיאה בעדכון התבנית');
          this.saving = false;
        },
      });
    } else {
      this.notificationApi.createTemplate({ orgUnitId: 1, ...value }).subscribe({
        next: () => {
          this.notification.success('התבנית נוצרה בהצלחה');
          this.router.navigate(['/notifications/templates']);
        },
        error: () => {
          this.notification.error('שגיאה ביצירת התבנית');
          this.saving = false;
        },
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/notifications/templates']);
  }
}
