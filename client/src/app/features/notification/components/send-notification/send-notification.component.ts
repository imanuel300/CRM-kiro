import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  NotificationTemplate,
  NotificationLog,
} from '../../models/notification.models';
import { NotificationApiService } from '../../services/notification-api.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsDropdownOption } from '@igds/angular';
import { IgdsTab } from '@igds/angular';

@Component({
  selector: 'app-send-notification',
  template: `
    <div class="page-header">
      <h1>שליחת הודעה ידנית</h1>
    </div>

    <igds-card>
      <igds-tabs
        [tabs]="tabs"
        [activeTab]="activeTab"
        (tabChange)="onTabChange($event)">
      </igds-tabs>

      <!-- Single send -->
      <div *ngIf="activeTab === 'single'" class="send-form">
        <form [formGroup]="singleForm" (ngSubmit)="onSendSingle()">
          <igds-dropdown
            label="תבנית"
            [options]="templateOptions"
            formControlName="templateId"
            [required]="true"
            placeholder="בחר תבנית...">
          </igds-dropdown>

          <igds-dropdown
            label="ערוץ שליחה"
            [options]="channelOptions"
            formControlName="channel"
            [required]="true">
          </igds-dropdown>

          <igds-input-field
            label="נמען (דוא״ל או טלפון)"
            formControlName="recipient"
            [required]="true"
            [error]="singleForm.get('recipient')?.touched && singleForm.get('recipient')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <igds-input-field
            label="מזהה מועמדות"
            type="number"
            formControlName="candidacyId"
            [required]="true"
            [error]="singleForm.get('candidacyId')?.touched && singleForm.get('candidacyId')?.hasError('required') ? 'שדה חובה' : ''">
          </igds-input-field>

          <div class="form-actions">
            <igds-button variant="primary" type="submit"
              [disabled]="singleForm.invalid || sending">
              שליחה
            </igds-button>
          </div>
        </form>
      </div>

      <!-- Bulk send -->
      <div *ngIf="activeTab === 'bulk'" class="send-form">
        <form [formGroup]="bulkForm" (ngSubmit)="onSendBulk()">
          <igds-dropdown
            label="תבנית"
            [options]="templateOptions"
            formControlName="templateId"
            [required]="true"
            placeholder="בחר תבנית...">
          </igds-dropdown>

          <igds-dropdown
            label="ערוץ שליחה"
            [options]="channelOptions"
            formControlName="channel"
            [required]="true">
          </igds-dropdown>

          <h3>נמענים</h3>
          <div formArrayName="recipients">
            <div *ngFor="let r of recipientsArray.controls; let i = index"
                 [formGroupName]="i" class="recipient-row">
              <igds-input-field
                label="מזהה מועמדות"
                type="number"
                formControlName="candidacyId">
              </igds-input-field>
              <igds-input-field
                label="נמען"
                formControlName="recipient">
              </igds-input-field>
              <igds-button variant="secondary" [iconOnly]="true"
                type="button" ariaLabel="הסרת נמען"
                (onClick)="removeRecipient(i)"
                *ngIf="recipientsArray.length > 1">
                <span igds-icon>✗</span>
              </igds-button>
            </div>
          </div>

          <igds-button variant="secondary" type="button" (onClick)="addRecipient()">
            הוספת נמען
          </igds-button>

          <div class="form-actions">
            <igds-button variant="primary" type="submit"
              [disabled]="bulkForm.invalid || sending">
              שליחה מרובה ({{ recipientsArray.length }})
            </igds-button>
          </div>
        </form>
      </div>
    </igds-card>
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
    .send-form {
      padding-block-start: var(--igds-space-16);
      display: flex;
      flex-direction: column;
      gap: var(--igds-space-12);
    }
    .send-form form {
      display: flex;
      flex-direction: column;
      gap: var(--igds-space-12);
    }
    .recipient-row {
      display: flex;
      gap: var(--igds-space-12);
      align-items: flex-end;
    }
    .recipient-row igds-input-field { flex: 1; }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
    h3 {
      margin: var(--igds-space-16) 0 var(--igds-space-8);
      font-family: var(--igds-font-family);
      color: var(--igds-text-secondary);
    }
  `],
})
export class SendNotificationComponent implements OnInit {
  templates: NotificationTemplate[] = [];
  sending = false;
  activeTab = 'single';

  tabs: IgdsTab[] = [
    { id: 'single', label: 'שליחה בודדת' },
    { id: 'bulk', label: 'שליחה מרובה' },
  ];

  templateOptions: IgdsDropdownOption[] = [];
  channelOptions: IgdsDropdownOption[] = [
    { value: 'Email', label: 'דוא"ל' },
    { value: 'Sms', label: 'SMS' },
  ];

  singleForm!: FormGroup;
  bulkForm!: FormGroup;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private notificationApi: NotificationApiService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.singleForm = this.fb.group({
      templateId: [null, Validators.required],
      channel: ['Email', Validators.required],
      recipient: ['', Validators.required],
      candidacyId: [null, Validators.required],
    });

    this.bulkForm = this.fb.group({
      templateId: [null, Validators.required],
      channel: ['Email', Validators.required],
      recipients: this.fb.array([this.createRecipientGroup()]),
    });

    this.singleForm.get('templateId')?.valueChanges.subscribe((templateId: number) => {
      this.onTemplateSelected(templateId, 'single');
    });

    this.bulkForm.get('templateId')?.valueChanges.subscribe((templateId: number) => {
      this.onTemplateSelected(templateId, 'bulk');
    });

    this.loadTemplates();
  }

  get recipientsArray(): FormArray {
    return this.bulkForm.get('recipients') as FormArray;
  }

  createRecipientGroup(): FormGroup {
    return this.fb.group({
      candidacyId: [null, Validators.required],
      recipient: ['', Validators.required],
    });
  }

  addRecipient(): void {
    this.recipientsArray.push(this.createRecipientGroup());
  }

  removeRecipient(index: number): void {
    this.recipientsArray.removeAt(index);
  }

  loadTemplates(): void {
    this.notificationApi.listTemplates().subscribe({
      next: (data: NotificationTemplate[]) => {
        this.templates = data;
        this.templateOptions = data.map(t => ({
          value: t.id,
          label: `${t.name} (${t.channel === 'Email' ? 'דוא"ל' : 'SMS'})`,
        }));
      },
      error: () => this.notification.error('שגיאה בטעינת תבניות'),
    });
  }

  onTemplateSelected(templateId: number, formType: 'single' | 'bulk'): void {
    const template = this.templates.find(t => t.id === templateId);
    if (template) {
      const form = formType === 'single' ? this.singleForm : this.bulkForm;
      form.patchValue({ channel: template.channel });
    }
  }

  onTabChange(tabId: string): void {
    this.activeTab = tabId;
    this.sending = false;
  }

  onSendSingle(): void {
    if (this.singleForm.invalid) return;
    this.sending = true;
    const value = this.singleForm.value;

    this.notificationApi.send({
      candidacyId: value.candidacyId,
      templateId: value.templateId,
      channel: value.channel,
      recipient: value.recipient,
    }).subscribe({
      next: () => {
        this.notification.success('ההודעה נשלחה בהצלחה');
        this.sending = false;
        this.singleForm.reset({ channel: 'Email' });
      },
      error: () => {
        this.notification.error('שגיאה בשליחת ההודעה');
        this.sending = false;
      },
    });
  }

  onSendBulk(): void {
    if (this.bulkForm.invalid) return;
    this.sending = true;
    const value = this.bulkForm.value;

    this.notificationApi.sendBulk({
      templateId: value.templateId,
      channel: value.channel,
      recipients: value.recipients,
    }).subscribe({
      next: (results: NotificationLog[]) => {
        this.notification.success(`נשלחו ${results.length} הודעות בהצלחה`);
        this.sending = false;
      },
      error: () => {
        this.notification.error('שגיאה בשליחת ההודעות');
        this.sending = false;
      },
    });
  }
}
