import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { debounceTime, distinctUntilChanged, switchMap, filter } from 'rxjs/operators';
import { ContactService } from '../../services/contact.service';
import { NotificationService } from '@core/services/notification.service';
import { Contact } from '../../models/contact.models';
import { IgdsDropdownOption } from '@igds/angular';

@Component({
  selector: 'app-contact-form',
  template: `
    <div class="page-header">
      <h1>{{ isEdit ? 'עריכת איש קשר' : 'יצירת איש קשר חדש' }}</h1>
    </div>

    <igds-card>
      <app-loading-spinner [loading]="loading"></app-loading-spinner>

      <form [formGroup]="form" (ngSubmit)="onSubmit()" *ngIf="!loading">
        <igds-input-field
          label="תעודת זהות"
          formControlName="idNumber"
          [required]="true"
          [error]="getIdNumberError()"
          [helperText]="duplicateContact ? '⚠ קיים איש קשר עם ת.ז. זו: ' + duplicateContact.firstName + ' ' + duplicateContact.lastName : ''">
        </igds-input-field>

        <div class="row">
          <div class="half-width">
            <igds-input-field
              label="שם פרטי"
              formControlName="firstName"
              [required]="true"
              [error]="form.get('firstName')?.touched && form.get('firstName')?.hasError('required') ? 'שדה חובה' : ''">
            </igds-input-field>
          </div>
          <div class="half-width">
            <igds-input-field
              label="שם משפחה"
              formControlName="lastName"
              [required]="true"
              [error]="form.get('lastName')?.touched && form.get('lastName')?.hasError('required') ? 'שדה חובה' : ''">
            </igds-input-field>
          </div>
        </div>

        <div class="row">
          <div class="half-width">
            <igds-date-picker
              label="תאריך לידה"
              formControlName="dateOfBirth">
            </igds-date-picker>
          </div>
          <div class="half-width">
            <igds-dropdown
              label="מגדר"
              formControlName="gender"
              placeholder="בחר מגדר..."
              [options]="genderOptions">
            </igds-dropdown>
          </div>
        </div>

        <igds-input-field
          label="כתובת"
          formControlName="address">
        </igds-input-field>

        <div class="row">
          <div class="half-width">
            <igds-input-field
              label="טלפון"
              formControlName="phone">
            </igds-input-field>
          </div>
          <div class="half-width">
            <igds-input-field
              label='דוא"ל'
              formControlName="email"
              type="email"
              [error]="form.get('email')?.touched && form.get('email')?.hasError('email') ? 'כתובת דוא&quot;ל לא תקינה' : ''">
            </igds-input-field>
          </div>
        </div>

        <div class="form-actions">
          <igds-button
            variant="primary"
            type="submit"
            [disabled]="form.invalid || saving || !!duplicateContact">
            {{ saving ? 'שומר...' : (isEdit ? 'עדכון' : 'יצירה') }}
          </igds-button>
          <igds-button variant="secondary" type="button" (onClick)="onCancel()">
            ביטול
          </igds-button>
        </div>
      </form>
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
    .row {
      display: flex;
      gap: var(--igds-space-16);
      margin-block-end: var(--igds-space-8);
    }
    .half-width { flex: 1; }
    .form-actions {
      display: flex;
      gap: var(--igds-space-8);
      margin-block-start: var(--igds-space-16);
    }
  `],
})
export class ContactFormComponent implements OnInit {
  form!: FormGroup;
  isEdit = false;
  loading = false;
  saving = false;
  duplicateContact: Contact | null = null;
  private contactId?: number;

  genderOptions: IgdsDropdownOption[] = [
    { value: 'male', label: 'זכר' },
    { value: 'female', label: 'נקבה' },
    { value: 'other', label: 'אחר' },
  ];

  constructor(
    private fb: FormBuilder,
    private route: ActivatedRoute,
    private router: Router,
    private contactService: ContactService,
    private notification: NotificationService
  ) {}

  ngOnInit(): void {
    this.form = this.fb.group({
      idNumber: ['', [Validators.required, Validators.pattern(/^\d{5,9}$/)]],
      firstName: ['', Validators.required],
      lastName: ['', Validators.required],
      dateOfBirth: [null],
      gender: [''],
      address: [''],
      phone: [''],
      email: ['', Validators.email],
    });

    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.isEdit = true;
      this.contactId = +id;
      this.form.get('idNumber')?.disable();
      this.loadContact();
    } else {
      this.setupDuplicateCheck();
    }
  }

  getIdNumberError(): string {
    const ctrl = this.form.get('idNumber');
    if (!ctrl?.touched) return '';
    if (ctrl.hasError('required')) return 'שדה חובה';
    if (ctrl.hasError('pattern')) return 'מספר ת.ז. לא תקין';
    return '';
  }

  private setupDuplicateCheck(): void {
    this.form.get('idNumber')?.valueChanges
      .pipe(
        debounceTime(500),
        distinctUntilChanged(),
        filter((val): val is string => !!val && val.length >= 5),
        switchMap((idNumber: string) => this.contactService.getByIdNumber(idNumber))
      )
      .subscribe({
        next: (contact: Contact | null) => {
          this.duplicateContact = contact;
        },
        error: () => {
          this.duplicateContact = null;
        },
      });
  }

  private loadContact(): void {
    this.loading = true;
    this.contactService.getById(this.contactId!).subscribe({
      next: (contact: Contact) => {
        this.form.patchValue({
          idNumber: contact.idNumber,
          firstName: contact.firstName,
          lastName: contact.lastName,
          dateOfBirth: contact.dateOfBirth ? new Date(contact.dateOfBirth) : null,
          gender: contact.gender || '',
          address: contact.address || '',
          phone: contact.phone || '',
          email: contact.email || '',
        });
        this.loading = false;
      },
      error: () => {
        this.notification.error('שגיאה בטעינת איש הקשר');
        this.loading = false;
      },
    });
  }

  onSubmit(): void {
    if (this.form.invalid || this.duplicateContact) return;
    this.saving = true;
    const value = this.form.getRawValue();

    if (this.isEdit) {
      this.contactService.update({ id: this.contactId!, ...value }).subscribe({
        next: () => {
          this.notification.success('איש הקשר עודכן בהצלחה');
          this.router.navigate(['/contacts', this.contactId]);
        },
        error: () => {
          this.notification.error('שגיאה בעדכון איש הקשר');
          this.saving = false;
        },
      });
    } else {
      this.contactService.create(value).subscribe({
        next: (created: Contact) => {
          this.notification.success('איש הקשר נוצר בהצלחה');
          this.router.navigate(['/contacts', created.id]);
        },
        error: (err: any) => {
          const msg = err?.error?.message || 'שגיאה ביצירת איש הקשר';
          this.notification.error(msg);
          this.saving = false;
        },
      });
    }
  }

  onCancel(): void {
    this.router.navigate(['/contacts']);
  }
}
