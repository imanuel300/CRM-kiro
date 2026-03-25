import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Contact } from '../../models/contact.models';
import { ContactService } from '../../services/contact.service';
import { NotificationService } from '@core/services/notification.service';
import { IgdsTab } from '@igds/angular';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-contact-detail',
  template: `
    <app-loading-spinner [loading]="loading"></app-loading-spinner>

    <div *ngIf="!loading && contact">
      <div class="page-header">
        <h1>{{ contact.firstName }} {{ contact.lastName }}</h1>
        <div class="header-actions">
          <igds-button variant="primary" (onClick)="onEdit()">
            עריכה
          </igds-button>
          <igds-button variant="secondary" (onClick)="onBack()">
            חזרה לרשימה
          </igds-button>
        </div>
      </div>

      <igds-tabs [tabs]="tabs" [activeTab]="activeTab" (tabChange)="onTabChange($event)"></igds-tabs>

      <div class="tab-content" *ngIf="activeTab === 'personal'">
        <igds-card>
          <div class="detail-grid">
            <div class="detail-item">
              <span class="label">תעודת זהות</span>
              <span class="value">{{ contact.idNumber }}</span>
            </div>
            <div class="detail-item">
              <span class="label">שם פרטי</span>
              <span class="value">{{ contact.firstName }}</span>
            </div>
            <div class="detail-item">
              <span class="label">שם משפחה</span>
              <span class="value">{{ contact.lastName }}</span>
            </div>
            <div class="detail-item">
              <span class="label">תאריך לידה</span>
              <span class="value">{{ contact.dateOfBirth ? (contact.dateOfBirth | hebrewDate) : '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">מגדר</span>
              <span class="value">{{ getGenderLabel(contact.gender) }}</span>
            </div>
            <div class="detail-item">
              <span class="label">כתובת</span>
              <span class="value">{{ contact.address || '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">טלפון</span>
              <span class="value">{{ contact.phone || '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">דוא"ל</span>
              <span class="value">{{ contact.email || '—' }}</span>
            </div>
            <div class="detail-item">
              <span class="label">נוצר בתאריך</span>
              <span class="value">{{ contact.createdAt | hebrewDate }}</span>
            </div>
            <div class="detail-item">
              <span class="label">עודכן לאחרונה</span>
              <span class="value">{{ contact.updatedAt | hebrewDate }}</span>
            </div>
          </div>
        </igds-card>
      </div>

      <div class="tab-content" *ngIf="activeTab === 'history'">
        <igds-card>
          <app-change-history [contactId]="contact.id"></app-change-history>
        </igds-card>
      </div>

      <div class="tab-content" *ngIf="activeTab === 'custom'">
        <igds-card>
          <igds-dropdown
            label="יחידה ארגונית"
            placeholder="בחר יחידה ארגונית"
            [options]="orgUnitOptions"
            [(ngModel)]="selectedOrgUnitId"
            (ngModelChange)="onOrgUnitChange()">
          </igds-dropdown>

          <app-custom-fields
            *ngIf="selectedOrgUnitId > 0"
            [contactId]="contact.id"
            [orgUnitId]="selectedOrgUnitId">
          </app-custom-fields>
        </igds-card>
      </div>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-block-end: var(--igds-space-16);
    }
    .page-header h1 {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .header-actions {
      display: flex;
      gap: var(--igds-space-8);
    }
    .tab-content {
      margin-block-start: var(--igds-space-16);
    }
    .detail-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: var(--igds-space-16);
    }
    .detail-item {
      display: flex;
      flex-direction: column;
    }
    .label {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xs);
      color: var(--igds-text-secondary);
      margin-block-end: var(--igds-space-4);
    }
    .value {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-md);
      color: var(--igds-text-primary);
    }
  `],
})
export class ContactDetailComponent implements OnInit {
  contact: Contact | null = null;
  loading = false;
  selectedOrgUnitId = 0;
  activeTab = 'personal';

  tabs: IgdsTab[] = [
    { id: 'personal', label: 'פרטים אישיים' },
    { id: 'history', label: 'היסטוריית שינויים' },
    { id: 'custom', label: 'שדות מותאמים' },
  ];

  orgUnitOptions: IgdsDropdownOption[] = [
    { value: 0, label: 'בחר יחידה ארגונית' },
  ];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private contactService: ContactService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadContact(+id);
    }
  }

  getGenderLabel(gender?: string): string {
    switch (gender) {
      case 'male': return 'זכר';
      case 'female': return 'נקבה';
      case 'other': return 'אחר';
      default: return '—';
    }
  }

  onTabChange(tabId: string): void {
    this.activeTab = tabId;
  }

  onEdit(): void {
    if (this.contact) {
      this.router.navigate(['/contacts', this.contact.id, 'edit']);
    }
  }

  onBack(): void {
    this.router.navigate(['/contacts']);
  }

  onOrgUnitChange(): void {
    // Triggers change detection for the custom-fields component
  }

  private loadContact(id: number): void {
    this.loading = true;
    this.contactService.getById(id).subscribe({
      next: (contact: Contact) => {
        this.contact = contact;
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת איש הקשר');
        this.loading = false;
      },
    });
  }
}
