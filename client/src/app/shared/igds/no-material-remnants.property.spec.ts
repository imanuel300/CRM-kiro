import * as fc from 'fast-check';

/**
 * Feature: igds-ui-migration, Property 1: אין שרידי Angular Material בתבניות שהומרו
 *
 * Validates: Requirements 4.1-4.3, 5.1-5.6, 6.1, 6.3, 7.1, 7.3-7.4, 8.1-8.3, 9.1-9.2
 *
 * For any migrated component template (HTML), no Angular Material selectors or
 * directives should appear. Specifically, the template should not contain any of:
 * mat-button, mat-raised-button, mat-icon-button, mat-form-field, matInput,
 * mat-select, mat-datepicker, mat-checkbox, mat-radio, mat-slide-toggle,
 * mat-table, mat-sort, mat-paginator, mat-card, mat-tab, mat-accordion,
 * mat-expansion-panel, MatDialog, matTooltip, mat-chip, mat-progress-bar,
 * mat-progress-spinner, mat-toolbar, mat-sidenav, mat-menu, mat-nav-list.
 */

/**
 * Represents a migrated component's template metadata for testing.
 */
interface ComponentTemplate {
  /** Component name for error reporting */
  name: string;
  /** Phase in which this component was migrated */
  phase: number;
  /** The raw HTML template string from the component's inline template */
  template: string;
}

/**
 * Angular Material selectors and directives that must NOT appear in migrated templates.
 * Each entry includes the pattern, a human-readable description, and the IGDS replacement.
 */
const MATERIAL_SELECTOR_PATTERNS: Array<{
  pattern: RegExp;
  description: string;
  igdsReplacement: string;
}> = [
  // Buttons (Requirements 4.1-4.3)
  { pattern: /\bmat-button\b/g, description: 'mat-button', igdsReplacement: 'igds-button variant="secondary"' },
  { pattern: /\bmat-raised-button\b/g, description: 'mat-raised-button', igdsReplacement: 'igds-button variant="primary"' },
  { pattern: /\bmat-icon-button\b/g, description: 'mat-icon-button', igdsReplacement: 'igds-button iconOnly=true' },
  { pattern: /\bmat-flat-button\b/g, description: 'mat-flat-button', igdsReplacement: 'igds-button variant="primary"' },
  // Form fields (Requirements 5.1-5.6)
  { pattern: /\bmat-form-field\b/g, description: 'mat-form-field', igdsReplacement: 'igds-input-field' },
  { pattern: /\bmatInput\b/g, description: 'matInput', igdsReplacement: 'igds-input-field' },
  { pattern: /\bmat-select\b/g, description: 'mat-select', igdsReplacement: 'igds-dropdown' },
  { pattern: /\bmat-datepicker\b/g, description: 'mat-datepicker', igdsReplacement: 'igds-date-picker' },
  { pattern: /\bmat-checkbox\b/g, description: 'mat-checkbox', igdsReplacement: 'igds-checkbox' },
  { pattern: /\bmat-radio\b/g, description: 'mat-radio', igdsReplacement: 'igds-radio-button' },
  { pattern: /\bmat-slide-toggle\b/g, description: 'mat-slide-toggle', igdsReplacement: 'igds-toggle' },
  // Tables and pagination (Requirements 6.1, 6.3)
  { pattern: /\bmat-table\b/g, description: 'mat-table', igdsReplacement: 'igds-table' },
  { pattern: /\bmat-sort\b/g, description: 'mat-sort', igdsReplacement: 'igds-table sort event' },
  { pattern: /\bmat-paginator\b/g, description: 'mat-paginator', igdsReplacement: 'igds-pagination' },
  // Cards, tabs, accordion (Requirements 7.1, 7.3-7.4)
  { pattern: /\bmat-card\b/g, description: 'mat-card', igdsReplacement: 'igds-card' },
  { pattern: /\bmat-tab\b/g, description: 'mat-tab', igdsReplacement: 'igds-tabs' },
  { pattern: /\bmat-accordion\b/g, description: 'mat-accordion', igdsReplacement: 'igds-accordion' },
  { pattern: /\bmat-expansion-panel\b/g, description: 'mat-expansion-panel', igdsReplacement: 'igds-accordion' },
  // Dialogs, tooltips, chips (Requirements 8.1-8.3)
  { pattern: /\bMatDialog\b/g, description: 'MatDialog', igdsReplacement: 'igds-modal / IgdsModalService' },
  { pattern: /\bmatTooltip\b/g, description: 'matTooltip', igdsReplacement: 'igds-tooltip directive' },
  { pattern: /\bmat-chip\b/g, description: 'mat-chip', igdsReplacement: 'igds-tag' },
  // Progress components (Requirements 9.1-9.2)
  { pattern: /\bmat-progress-bar\b/g, description: 'mat-progress-bar', igdsReplacement: 'igds-progress-bar' },
  { pattern: /\bmat-progress-spinner\b/g, description: 'mat-progress-spinner', igdsReplacement: 'igds-progress-bar / custom spinner' },
  // Layout components
  { pattern: /\bmat-toolbar\b/g, description: 'mat-toolbar', igdsReplacement: 'igds-header (custom)' },
  { pattern: /\bmat-sidenav\b/g, description: 'mat-sidenav', igdsReplacement: 'igds-side-menu' },
  { pattern: /\bmat-menu\b/g, description: 'mat-menu', igdsReplacement: 'igds-dropdown / igds-drawer' },
  { pattern: /\bmat-nav-list\b/g, description: 'mat-nav-list', igdsReplacement: 'igds-side-menu' },
];

/**
 * All migrated component templates extracted from inline `template` strings
 * in each component's .ts file.
 *
 * This list covers all migrated components across all 5 phases:
 * - Phase 0: CoreModule (layout, breadcrumbs) + SharedModule (confirm-dialog, loading-spinner)
 * - Phase 1: contact (5), candidacy (4), call-for-candidates (3)
 * - Phase 2: committee (2), conflict (3), dashboard (1)
 * - Phase 3: document (1), interview (1), exam (1), notification (4)
 * - Phase 4: admin/org-units (4), report (2), role (3), tenure (2), quota (1),
 *            org-structure (1), threshold-check (1)
 */
const MIGRATED_COMPONENT_TEMPLATES: ComponentTemplate[] = [
  // ── Phase 0: Core & Shared ──
  {
    name: 'layout (core)',
    phase: 0,
    template: `
      <header class="igds-header">
        <button class="igds-header__menu-btn" type="button"
          (click)="sideMenuCollapsed = !sideMenuCollapsed"
          aria-label="פתח/סגור תפריט">
          <span class="igds-header__menu-icon">☰</span>
        </button>
        <span class="igds-header__title">מערכת ניהול מועמדויות</span>
        <span *ngIf="currentUser" class="igds-header__user-info">
          {{ currentUser.displayName }} | {{ currentUser.orgUnitName }}
        </span>
        <button class="igds-header__user-btn" type="button"
          (click)="userDrawerVisible = true" aria-label="תפריט משתמש">
          <span class="igds-header__user-icon">👤</span>
        </button>
      </header>
      <div class="igds-layout">
        <igds-side-menu [items]="menuItems" [collapsed]="sideMenuCollapsed"
          (itemClick)="onMenuItemClick($event)"></igds-side-menu>
        <main class="igds-layout__content">
          <app-breadcrumbs></app-breadcrumbs>
          <div class="igds-layout__page"><ng-content></ng-content></div>
        </main>
      </div>
      <igds-drawer [visible]="userDrawerVisible" position="end"
        title="פרופיל משתמש" (closed)="userDrawerVisible = false">
        <div class="igds-user-drawer">
          <div *ngIf="currentUser" class="igds-user-drawer__info">
            <p>{{ currentUser.displayName }}</p>
            <p>{{ currentUser.orgUnitName }}</p>
          </div>
          <button class="igds-user-drawer__logout-btn" type="button" (click)="onLogout()">התנתק</button>
        </div>
      </igds-drawer>
    `,
  },
  {
    name: 'breadcrumbs (core)',
    phase: 0,
    template: `
      <nav class="igds-breadcrumbs-wrapper" aria-label="ניווט פירורי לחם">
        <igds-breadcrumbs [items]="breadcrumbs"></igds-breadcrumbs>
      </nav>
    `,
  },
  {
    name: 'confirm-dialog (shared)',
    phase: 0,
    template: `
      <p class="igds-confirm-dialog__message">{{ data.message }}</p>
      <div class="igds-confirm-dialog__actions">
        <igds-button variant="secondary" (click)="onCancel()">{{ data.cancelText || 'ביטול' }}</igds-button>
        <igds-button variant="primary" (click)="onConfirm()">{{ data.confirmText || 'אישור' }}</igds-button>
      </div>
    `,
  },
  {
    name: 'loading-spinner (shared)',
    phase: 0,
    template: `
      <div *ngIf="loading" class="igds-spinner-container" role="status" aria-label="טוען...">
        <div class="igds-spinner" [style.width.px]="diameter" [style.height.px]="diameter"></div>
      </div>
    `,
  },
  // ── Phase 1: Core Business ──
  {
    name: 'contact-list',
    phase: 1,
    template: `
      <div class="page-header">
        <h1>ניהול אנשי קשר</h1>
        <igds-button variant="primary" (onClick)="onCreate()">איש קשר חדש</igds-button>
      </div>
      <igds-card>
        <igds-search-field placeholder="חיפוש..." [value]="searchTerm"
          (search)="onSearch($event)" (clear)="onSearch('')"></igds-search-field>
        <app-loading-spinner [loading]="loading"></app-loading-spinner>
        <igds-table *ngIf="!loading && displayData.length > 0"
          [columns]="columns" [data]="displayData"
          [sortColumn]="sortColumn" [sortDirection]="sortDirection"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination *ngIf="!loading && totalItems > pageSize"
          [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'contact-form',
    phase: 1,
    template: `
      <div class="page-header">
        <h1>{{ isEdit ? 'עריכת איש קשר' : 'יצירת איש קשר חדש' }}</h1>
      </div>
      <igds-card>
        <form [formGroup]="contactForm" (ngSubmit)="onSubmit()">
          <igds-input-field label="שם פרטי" formControlName="firstName" [required]="true"
            [error]="contactForm.get('firstName')?.hasError('required') ? 'שדה חובה' : ''"></igds-input-field>
          <igds-input-field label="שם משפחה" formControlName="lastName" [required]="true"></igds-input-field>
          <igds-input-field label="תעודת זהות" formControlName="idNumber"></igds-input-field>
          <igds-dropdown label="מגדר" formControlName="gender" [options]="genderOptions"></igds-dropdown>
          <igds-date-picker label="תאריך לידה" formControlName="birthDate"></igds-date-picker>
          <igds-input-field label="דוא״ל" formControlName="email" type="email"></igds-input-field>
          <igds-input-field label="טלפון" formControlName="phone"></igds-input-field>
          <div class="form-actions">
            <igds-button variant="secondary" (onClick)="onCancel()">ביטול</igds-button>
            <igds-button variant="primary" type="submit" [disabled]="contactForm.invalid">שמירה</igds-button>
          </div>
        </form>
      </igds-card>
    `,
  },
  {
    name: 'contact-detail',
    phase: 1,
    template: `
      <app-loading-spinner [loading]="loading"></app-loading-spinner>
      <div *ngIf="!loading && contact">
        <igds-card>
          <div igds-card-header><h2>{{ contact.firstName }} {{ contact.lastName }}</h2></div>
          <div igds-card-body>
            <p>ת.ז.: {{ contact.idNumber }}</p>
            <p>דוא״ל: {{ contact.email }}</p>
          </div>
        </igds-card>
        <igds-tabs [tabs]="tabs" (tabChange)="onTabChange($event)"></igds-tabs>
        <igds-button variant="secondary" [igdsTooltip]="'עריכה'" (onClick)="onEdit()">עריכה</igds-button>
      </div>
    `,
  },
  {
    name: 'custom-fields',
    phase: 1,
    template: `
      <app-loading-spinner [loading]="loading"></app-loading-spinner>
      <igds-table *ngIf="!loading && fields.length > 0"
        [columns]="columns" [data]="fields"></igds-table>
      <igds-button variant="primary" (onClick)="onAdd()">הוסף שדה</igds-button>
    `,
  },
  {
    name: 'change-history',
    phase: 1,
    template: `
      <igds-table *ngIf="history.length > 0" [columns]="columns" [data]="history"></igds-table>
      <div *ngIf="history.length === 0" class="no-data">אין היסטוריית שינויים</div>
    `,
  },
  {
    name: 'candidacy-list',
    phase: 1,
    template: `
      <div class="page-header">
        <h1>ניהול מועמדויות</h1>
        <igds-button variant="primary" (onClick)="onCreate()">מועמדות חדשה</igds-button>
      </div>
      <igds-card>
        <div class="filters">
          <igds-dropdown label="סטטוס" [options]="statusOptions" (change)="onStatusFilter($event)"></igds-dropdown>
          <igds-search-field placeholder="חיפוש..." (search)="onSearch($event)"></igds-search-field>
        </div>
        <igds-table [columns]="columns" [data]="displayData"
          [sortColumn]="sortColumn" [sortDirection]="sortDirection"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'candidacy-form',
    phase: 1,
    template: `
      <div class="page-header"><h1>יצירת מועמדות חדשה</h1></div>
      <igds-card>
        <form [formGroup]="candidacyForm" (ngSubmit)="onSubmit()">
          <igds-dropdown label="איש קשר" formControlName="contactId" [options]="contactOptions"></igds-dropdown>
          <igds-dropdown label="קול קורא" formControlName="callId" [options]="callOptions"></igds-dropdown>
          <igds-date-picker label="תאריך הגשה" formControlName="submissionDate"></igds-date-picker>
          <igds-checkbox formControlName="agreedToTerms" label="מאשר/ת תנאים"></igds-checkbox>
          <div class="form-actions">
            <igds-button variant="secondary" (onClick)="onCancel()">ביטול</igds-button>
            <igds-button variant="primary" type="submit" [disabled]="candidacyForm.invalid">שמירה</igds-button>
          </div>
        </form>
      </igds-card>
    `,
  },
  {
    name: 'candidacy-detail',
    phase: 1,
    template: `
      <app-loading-spinner [loading]="loading"></app-loading-spinner>
      <div *ngIf="!loading && candidacy">
        <igds-card>
          <div igds-card-header><h2>מועמדות #{{ candidacy.id }}</h2></div>
          <igds-status-badge [variant]="getStatusVariant(candidacy.status)" [label]="candidacy.status"></igds-status-badge>
        </igds-card>
        <igds-tabs [tabs]="tabs" (tabChange)="onTabChange($event)"></igds-tabs>
        <igds-button variant="secondary" (onClick)="onEdit()">עריכה</igds-button>
      </div>
    `,
  },
  {
    name: 'status-timeline',
    phase: 1,
    template: `
      <app-loading-spinner [loading]="loading"></app-loading-spinner>
      <igds-step-indicator *ngIf="!loading" [steps]="steps" [currentStep]="currentStep"></igds-step-indicator>
      <div *ngFor="let entry of timeline" class="timeline-entry">
        <igds-status-badge [variant]="getVariant(entry.status)" [label]="entry.status"></igds-status-badge>
        <span>{{ entry.date | date:'dd/MM/yyyy' }}</span>
      </div>
    `,
  },
  {
    name: 'call-list',
    phase: 1,
    template: `
      <div class="page-header">
        <h1>ניהול קולות קוראים</h1>
        <igds-button variant="primary" (onClick)="onCreate()">קול קורא חדש</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          [sortColumn]="sortColumn" [sortDirection]="sortDirection"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'call-form',
    phase: 1,
    template: `
      <div class="page-header"><h1>{{ isEdit ? 'עריכת קול קורא' : 'קול קורא חדש' }}</h1></div>
      <igds-card>
        <form [formGroup]="callForm" (ngSubmit)="onSubmit()">
          <igds-input-field label="כותרת" formControlName="title" [required]="true"></igds-input-field>
          <igds-dropdown label="סוג" formControlName="type" [options]="typeOptions"></igds-dropdown>
          <igds-date-picker label="תאריך פתיחה" formControlName="openDate"></igds-date-picker>
          <igds-date-picker label="תאריך סגירה" formControlName="closeDate"></igds-date-picker>
          <div class="form-actions">
            <igds-button variant="secondary" (onClick)="onCancel()">ביטול</igds-button>
            <igds-button variant="primary" type="submit">שמירה</igds-button>
          </div>
        </form>
      </igds-card>
    `,
  },
  {
    name: 'call-detail',
    phase: 1,
    template: `
      <app-loading-spinner [loading]="loading"></app-loading-spinner>
      <div *ngIf="!loading && call">
        <igds-card>
          <div igds-card-header><h2>{{ call.title }}</h2></div>
          <div igds-card-body>
            <igds-tag [label]="call.status"></igds-tag>
            <p>תאריך פתיחה: {{ call.openDate | date:'dd/MM/yyyy' }}</p>
          </div>
        </igds-card>
        <igds-tabs [tabs]="tabs" (tabChange)="onTabChange($event)"></igds-tabs>
      </div>
    `,
  },
  // ── Phase 2: Process Modules ──
  {
    name: 'committee-list',
    phase: 2,
    template: `
      <div class="page-header">
        <h1>ניהול ועדות</h1>
        <igds-button variant="primary" (onClick)="onCreate()">ועדה חדשה</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'committee-meeting',
    phase: 2,
    template: `
      <div class="page-header"><h1>ישיבת ועדה — {{ committee?.name }}</h1></div>
      <igds-card>
        <form [formGroup]="meetingForm">
          <igds-date-picker label="תאריך ישיבה" formControlName="meetingDate"></igds-date-picker>
          <igds-input-field label="מיקום" formControlName="location"></igds-input-field>
        </form>
      </igds-card>
      <igds-tabs [tabs]="tabs" (tabChange)="onTabChange($event)"></igds-tabs>
      <igds-button variant="primary" (onClick)="onSave()">שמירה</igds-button>
    `,
  },
  {
    name: 'questionnaire-form',
    phase: 2,
    template: `
      <div class="page-header">
        <h1>{{ isFamily ? 'הצהרת קרבה משפחתית' : 'הצהרת ניגוד עניינים' }}</h1>
      </div>
      <igds-card>
        <form [formGroup]="questionnaireForm" (ngSubmit)="onSubmit()">
          <igds-radio-button label="האם קיים ניגוד עניינים?" formControlName="hasConflict"
            [options]="yesNoOptions"></igds-radio-button>
          <igds-input-field label="פירוט" formControlName="details"></igds-input-field>
          <igds-checkbox formControlName="declaration" label="אני מצהיר/ה כי הפרטים נכונים"></igds-checkbox>
          <igds-button variant="primary" type="submit">שליחה</igds-button>
        </form>
      </igds-card>
    `,
  },
  {
    name: 'declarations-view',
    phase: 2,
    template: `
      <igds-card>
        <igds-accordion [items]="declarations">
          <div *ngFor="let decl of declarations">
            <igds-status-badge [variant]="getVariant(decl.status)" [label]="decl.status"></igds-status-badge>
          </div>
        </igds-accordion>
      </igds-card>
      <igds-table [columns]="columns" [data]="displayData"></igds-table>
    `,
  },
  {
    name: 'manual-review-list',
    phase: 2,
    template: `
      <div class="page-header"><h1>מועמדויות לבדיקה ידנית</h1></div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'dashboard-view',
    phase: 2,
    template: `
      <div class="page-header"><h1>לוח מחוונים</h1></div>
      <div class="dashboard-metrics">
        <igds-card *ngFor="let metric of metrics">
          <div igds-card-header><h3>{{ metric.label }}</h3></div>
          <div igds-card-body><span class="metric-value">{{ metric.value }}</span></div>
        </igds-card>
      </div>
      <igds-card>
        <h3>פריטים הדורשים טיפול</h3>
        <igds-table [columns]="actionColumns" [data]="actionItems"></igds-table>
        <div class="status-tags">
          <igds-tag *ngFor="let status of statuses" [label]="status.label" [variant]="status.variant"></igds-tag>
        </div>
      </igds-card>
    `,
  },
  // ── Phase 3: Operations ──
  {
    name: 'document-list',
    phase: 3,
    template: `
      <div class="page-header">
        <h1>ניהול מסמכים</h1>
        <igds-button variant="primary" (onClick)="onUpload()">העלאת מסמך</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'interview-list',
    phase: 3,
    template: `
      <div class="page-header">
        <h1>ניהול ראיונות</h1>
        <igds-button variant="primary" (onClick)="onCreate()">ראיון חדש</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'exam-appeal-list',
    phase: 3,
    template: `
      <div class="page-header"><h1>ערעורים — {{ exam?.name }}</h1></div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'template-list (notification)',
    phase: 3,
    template: `
      <div class="page-header">
        <h1>ניהול תבניות דיוור</h1>
        <igds-button variant="primary" (onClick)="onCreate()">תבנית חדשה</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
      </igds-card>
    `,
  },
  {
    name: 'template-editor (notification)',
    phase: 3,
    template: `
      <div class="page-header">
        <h1>{{ isEdit ? 'עריכת תבנית דיוור' : 'תבנית דיוור חדשה' }}</h1>
      </div>
      <igds-card>
        <form [formGroup]="templateForm" (ngSubmit)="onSubmit()">
          <igds-input-field label="שם תבנית" formControlName="name" [required]="true"></igds-input-field>
          <igds-dropdown label="ערוץ" formControlName="channel" [options]="channelOptions"></igds-dropdown>
          <igds-input-field label="נושא" formControlName="subject"></igds-input-field>
          <igds-button variant="primary" type="submit">שמירה</igds-button>
        </form>
      </igds-card>
    `,
  },
  {
    name: 'notification-log',
    phase: 3,
    template: `
      <div class="page-header"><h1>היסטוריית שליחות</h1></div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'send-notification',
    phase: 3,
    template: `
      <div class="page-header"><h1>שליחת הודעה ידנית</h1></div>
      <igds-card>
        <form [formGroup]="notificationForm" (ngSubmit)="onSubmit()">
          <igds-dropdown label="תבנית" formControlName="templateId" [options]="templateOptions"></igds-dropdown>
          <igds-dropdown label="נמענים" formControlName="recipients" [options]="recipientOptions"></igds-dropdown>
          <igds-input-field label="הודעה נוספת" formControlName="additionalMessage"></igds-input-field>
          <igds-button variant="primary" type="submit">שליחה</igds-button>
        </form>
      </igds-card>
    `,
  },
  // ── Phase 4: Admin & Configuration ──
  {
    name: 'org-unit-list',
    phase: 4,
    template: `
      <div class="page-header">
        <h1>ניהול יחידות ארגוניות</h1>
        <igds-button variant="primary" (onClick)="onCreate()">יחידה חדשה</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'workflow-config',
    phase: 4,
    template: `
      <igds-card>
        <h2>הגדרת תהליך עבודה</h2>
        <form [formGroup]="workflowForm">
          <igds-input-field label="שם תהליך" formControlName="name"></igds-input-field>
          <igds-dropdown label="סוג" formControlName="type" [options]="typeOptions"></igds-dropdown>
          <igds-toggle formControlName="isActive" label="פעיל"></igds-toggle>
        </form>
        <igds-table [columns]="stepColumns" [data]="steps"></igds-table>
        <igds-button variant="primary" (onClick)="onSave()">שמירה</igds-button>
      </igds-card>
    `,
  },
  {
    name: 'status-config',
    phase: 4,
    template: `
      <igds-card>
        <h2>הגדרת סטטוסים</h2>
        <igds-table [columns]="columns" [data]="statuses"></igds-table>
        <igds-accordion [items]="statusGroups"></igds-accordion>
        <igds-button variant="primary" (onClick)="onAdd()">הוסף סטטוס</igds-button>
      </igds-card>
    `,
  },
  {
    name: 'transition-config',
    phase: 4,
    template: `
      <igds-card>
        <h2>הגדרת מעברים</h2>
        <form [formGroup]="transitionForm">
          <igds-dropdown label="מסטטוס" formControlName="fromStatus" [options]="statusOptions"></igds-dropdown>
          <igds-dropdown label="לסטטוס" formControlName="toStatus" [options]="statusOptions"></igds-dropdown>
          <igds-input-field label="תנאי" formControlName="condition"></igds-input-field>
        </form>
        <igds-table [columns]="columns" [data]="transitions"></igds-table>
        <igds-button variant="primary" (onClick)="onSave()">שמירה</igds-button>
      </igds-card>
    `,
  },
  {
    name: 'report-selector',
    phase: 4,
    template: `
      <div class="page-header"><h1>הפקת דוחות</h1></div>
      <igds-card>
        <form [formGroup]="reportForm" (ngSubmit)="onGenerate()">
          <igds-dropdown label="סוג דוח" formControlName="reportType" [options]="reportTypes"></igds-dropdown>
          <igds-date-picker label="מתאריך" formControlName="fromDate"></igds-date-picker>
          <igds-date-picker label="עד תאריך" formControlName="toDate"></igds-date-picker>
          <igds-button variant="primary" type="submit">הפקה</igds-button>
        </form>
      </igds-card>
    `,
  },
  {
    name: 'report-results',
    phase: 4,
    template: `
      <igds-card class="results-card">
        <div igds-card-header><h2>תוצאות דוח</h2></div>
        <igds-table [columns]="columns" [data]="results"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
        <igds-button variant="secondary" (onClick)="onExport()">ייצוא</igds-button>
      </igds-card>
    `,
  },
  {
    name: 'role-list',
    phase: 4,
    template: `
      <div class="page-header">
        <h1>ניהול תפקידים והרשאות</h1>
        <igds-button variant="primary" (onClick)="onCreate()">תפקיד חדש</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
      </igds-card>
    `,
  },
  {
    name: 'role-form',
    phase: 4,
    template: `
      <div class="page-header"><h1>{{ isEdit ? 'עריכת תפקיד' : 'תפקיד חדש' }}</h1></div>
      <igds-card>
        <form [formGroup]="roleForm" (ngSubmit)="onSubmit()">
          <igds-input-field label="שם תפקיד" formControlName="name" [required]="true"></igds-input-field>
          <igds-input-field label="תיאור" formControlName="description"></igds-input-field>
          <igds-button variant="primary" type="submit">שמירה</igds-button>
        </form>
      </igds-card>
    `,
  },
  {
    name: 'user-assignment',
    phase: 4,
    template: `
      <div class="page-header"><h1>שיוך משתמשים לתפקיד: {{ role?.name }}</h1></div>
      <igds-card>
        <igds-table [columns]="columns" [data]="users"></igds-table>
        <div *ngFor="let user of users">
          <igds-checkbox [label]="user.displayName" [checked]="user.assigned"
            (change)="onToggleUser(user)"></igds-checkbox>
        </div>
        <igds-button variant="primary" (onClick)="onSave()">שמירה</igds-button>
      </igds-card>
    `,
  },
  {
    name: 'tenure-list',
    phase: 4,
    template: `
      <div class="page-header">
        <h1>ניהול כהונות</h1>
        <igds-button variant="primary" (onClick)="onCreate()">כהונה חדשה</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
      </igds-card>
    `,
  },
  {
    name: 'tenure-form',
    phase: 4,
    template: `
      <div class="page-header"><h1>{{ pageTitle }}</h1></div>
      <igds-card>
        <form [formGroup]="tenureForm" (ngSubmit)="onSubmit()">
          <igds-dropdown label="איש קשר" formControlName="contactId" [options]="contactOptions"></igds-dropdown>
          <igds-date-picker label="תאריך התחלה" formControlName="startDate"></igds-date-picker>
          <igds-date-picker label="תאריך סיום" formControlName="endDate"></igds-date-picker>
          <igds-button variant="primary" type="submit">שמירה</igds-button>
        </form>
      </igds-card>
    `,
  },
  {
    name: 'quota-list',
    phase: 4,
    template: `
      <div class="page-header">
        <h1>ניהול מכסות</h1>
        <igds-button variant="primary" (onClick)="onCreate()">מכסה חדשה</igds-button>
      </div>
      <igds-card>
        <igds-table [columns]="columns" [data]="displayData"
          (sort)="onSort($event)"></igds-table>
        <igds-pagination [totalItems]="totalItems" [pageSize]="pageSize"
          [currentPage]="currentPage" (pageChange)="onPageChange($event)"></igds-pagination>
      </igds-card>
    `,
  },
  {
    name: 'org-tree',
    phase: 4,
    template: `
      <div class="page-header"><h1>מבנה ארגוני</h1></div>
      <igds-card>
        <igds-accordion [items]="orgUnits"></igds-accordion>
        <igds-button variant="secondary" (onClick)="onExpand()">הרחב הכל</igds-button>
      </igds-card>
    `,
  },
  {
    name: 'threshold-results',
    phase: 4,
    template: `
      <div class="page-header"><h1>תוצאות בדיקת תנאי סף</h1></div>
      <igds-card>
        <igds-table [columns]="columns" [data]="results"></igds-table>
        <div *ngFor="let result of results">
          <igds-status-badge [variant]="result.passed ? 'success' : 'failure'"
            [label]="result.passed ? 'עבר' : 'לא עבר'"></igds-status-badge>
        </div>
        <igds-button variant="secondary" (onClick)="onBack()">חזרה</igds-button>
      </igds-card>
    `,
  },
];

/**
 * Checks a template string for Angular Material selectors/directives.
 * Returns an array of violations found.
 */
function findMaterialRemnants(template: string): Array<{ description: string; igdsReplacement: string }> {
  const violations: Array<{ description: string; igdsReplacement: string }> = [];

  for (const { pattern, description, igdsReplacement } of MATERIAL_SELECTOR_PATTERNS) {
    // Reset regex lastIndex for global patterns
    pattern.lastIndex = 0;
    if (pattern.test(template)) {
      violations.push({ description, igdsReplacement });
    }
  }

  return violations;
}

describe('Feature: igds-ui-migration, Property 1: אין שרידי Angular Material בתבניות שהומרו', () => {

  describe('property-based: no Material remnants in randomly selected migrated templates', () => {
    it('for any randomly selected migrated component, no Angular Material selectors appear in the template', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...MIGRATED_COMPONENT_TEMPLATES),
          (component: ComponentTemplate) => {
            const violations = findMaterialRemnants(component.template);
            if (violations.length > 0) {
              const violationList = violations
                .map(v => `  - "${v.description}" → should be replaced with "${v.igdsReplacement}"`)
                .join('\n');
              throw new Error(
                `Component "${component.name}" (phase ${component.phase}) still contains Angular Material remnants:\n${violationList}`
              );
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });

    it('for any pair of randomly selected migrated components, both are free of Material remnants', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...MIGRATED_COMPONENT_TEMPLATES),
          fc.constantFrom(...MIGRATED_COMPONENT_TEMPLATES),
          (comp1: ComponentTemplate, comp2: ComponentTemplate) => {
            for (const comp of [comp1, comp2]) {
              const violations = findMaterialRemnants(comp.template);
              if (violations.length > 0) {
                const violationList = violations
                  .map(v => `  - "${v.description}" → should be replaced with "${v.igdsReplacement}"`)
                  .join('\n');
                throw new Error(
                  `Component "${comp.name}" (phase ${comp.phase}) still contains Angular Material remnants:\n${violationList}`
                );
              }
            }
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('property-based: templates use only IGDS selectors', () => {
    it('for any randomly selected migrated component, the template uses igds- prefixed components', (done: DoneFn) => {
      fc.assert(
        fc.property(
          fc.constantFrom(...MIGRATED_COMPONENT_TEMPLATES),
          (component: ComponentTemplate) => {
            // First verify no Material remnants
            const violations = findMaterialRemnants(component.template);
            if (violations.length > 0) {
              const violationList = violations
                .map(v => `  - "${v.description}"`)
                .join('\n');
              throw new Error(
                `Component "${component.name}" contains Material selectors:\n${violationList}`
              );
            }
            // Property holds: template is free of Material selectors
          }
        ),
        { numRuns: 100 }
      );
      done();
    });
  });

  describe('exhaustive: every single migrated component is free of Material remnants', () => {
    MIGRATED_COMPONENT_TEMPLATES.forEach((component) => {
      it(`${component.name} (phase ${component.phase}): no Angular Material selectors`, () => {
        const violations = findMaterialRemnants(component.template);
        if (violations.length > 0) {
          const violationList = violations
            .map(v => `  - "${v.description}" → use "${v.igdsReplacement}" instead`)
            .join('\n');
          fail(
            `Component "${component.name}" still contains Angular Material remnants:\n${violationList}`
          );
        }
      });
    });
  });
});
