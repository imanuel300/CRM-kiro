# תוכנית מימוש: מיגרציה מ-Angular Material ל-IGDS

## סקירה

תוכנית זו מפרטת את משימות הקוד למיגרציה הדרגתית של כל רכיבי Angular Material לרכיבי IGDS הקיימים ב-`client/src/app/shared/igds/`. המיגרציה מתבצעת ב-5 פאזות, כאשר בכל פאזה המערכת נשארת פעילה. בסיום כל הפאזות מתבצע ניקוי מלא של תלויות Angular Material.

## משימות

- [x] 1. פאזה 0 — תשתית: שירותי עזר (IgdsModalService, IgdsToastService)
  - [x] 1.1 יצירת IgdsModalService
    - ליצור קובץ `client/src/app/shared/igds/services/igds-modal.service.ts`
    - לממש `open<T>(config: IgdsModalConfig): IgdsModalRef<T>` עם dynamic component creation
    - לממש ממשקי `IgdsModalConfig` ו-`IgdsModalRef<T>` כמתואר בעיצוב
    - `afterClosed()` מחזיר `Observable<T | undefined>`, `close(result?)` סוגר ומשחרר
    - לרשום את השירות ב-`providedIn: 'root'`
    - _דרישות: 8.1, 8.4_

  - [x] 1.2 יצירת IgdsToastService
    - ליצור קובץ `client/src/app/shared/igds/services/igds-toast.service.ts`
    - לממש מתודות `success()`, `error()`, `warning()`, `info()` עם ניהול מופע toast גלובלי
    - לרשום את השירות ב-`providedIn: 'root'`
    - _דרישות: 3.3, 8.5_

  - [x] 1.3 כתיבת בדיקת תכונה עבור IgdsModalService
    - **Property 6: זרימת נתונים בדיאלוגים (round-trip)**
    - **מאמת: דרישות 8.4, 17.4**

  - [x] 1.4 כתיבת בדיקת תכונה עבור IgdsToastService
    - **Property 7: סוגי הודעות toast**
    - **מאמת: דרישות 8.5**

- [x] 2. פאזה 0 — תשתית: מיגרציית CoreModule (Layout, Breadcrumbs)
  - [x] 2.1 מיגרציית LayoutComponent
    - בקובץ `client/src/app/core/layout/layout.component.ts` (ותבנית):
    - להחליף `mat-toolbar` ב-header מותאם עם Design Tokens של IGDS
    - להחליף `mat-sidenav` + `mat-nav-list` ב-`igds-side-menu` עם מיפוי `NavItem[]` ל-`IgdsSideMenuItem[]`
    - להחליף `mat-menu` (תפריט משתמש) ב-`igds-dropdown` או `igds-drawer`
    - לשמור על סינון ניווט לפי הרשאות (17 פריטים)
    - להסיר ייבואי Material מ-CoreModule
    - _דרישות: 2.1, 2.2, 2.3, 2.5, 2.6, 10.1, 10.4_

  - [x] 2.2 מיגרציית BreadcrumbsComponent
    - בקובץ `client/src/app/core/layout/breadcrumbs/` — להחליף מימוש קיים ב-`igds-breadcrumbs`
    - _דרישות: 2.4_

  - [x] 2.3 כתיבת בדיקת תכונה עבור סינון ניווט לפי הרשאות
    - **Property 13: סינון ניווט לפי הרשאות**
    - **מאמת: דרישות 2.5, 17.5**

- [x] 3. פאזה 0 — תשתית: מיגרציית רכיבי SharedModule
  - [x] 3.1 מיגרציית ConfirmDialogComponent
    - בקובץ `client/src/app/shared/components/confirm-dialog/confirm-dialog.component.ts` (ותבנית):
    - להחליף `MatDialog` / `mat-dialog-*` ב-`igds-modal` (או שימוש ב-`IgdsModalService`)
    - להחליף `mat-button` / `mat-raised-button` ב-`igds-button` (variant secondary/primary)
    - לשמור על ממשק `ConfirmDialogData` (title, message, confirmText, cancelText)
    - _דרישות: 3.1, 3.2, 3.5, 4.1, 4.2_

  - [x] 3.2 מיגרציית NotificationService
    - בקובץ `client/src/app/core/services/notification.service.ts`:
    - להחליף שימוש ב-`MatSnackBar` בשימוש ב-`IgdsToastService`
    - _דרישות: 3.3_

  - [x] 3.3 מיגרציית LoadingSpinnerComponent
    - בקובץ `client/src/app/shared/components/loading-spinner/loading-spinner.component.ts` (ותבנית):
    - להחליף `mat-progress-spinner` ב-`igds-progress-bar` או אנימציית טעינה מותאמת עם Design Tokens
    - _דרישות: 3.4, 9.2_

  - [x] 3.4 כתיבת בדיקת תכונה עבור ConfirmDialogData
    - **Property 14: שמירה על ממשק ConfirmDialogData**
    - **מאמת: דרישות 3.5**

- [x] 4. נקודת ביקורת — פאזה 0
  - לוודא שכל הבדיקות עוברות. לשאול את המשתמש אם יש שאלות.


- [x] 5. פאזה 1 — ליבה עסקית: מודול contact
  - [x] 5.1 מיגרציית contact-list
    - בקובץ `client/src/app/features/contact/components/contact-list/`:
    - להחליף `mat-table` + `MatTableDataSource` + `mat-sort` ב-`igds-table` עם `IgdsTableColumn[]` ו-sort event
    - להחליף `mat-paginator` ב-`igds-pagination`
    - להחליף שדה חיפוש ב-`igds-search-field`
    - להחליף כפתורים ב-`igds-button`
    - לנהל סינון/מיון/עימוד ברכיב עצמו (במקום MatTableDataSource)
    - _דרישות: 13.1, 13.4, 6.1, 6.2, 6.3, 6.5, 17.3, 17.6_

  - [x] 5.2 מיגרציית contact-form
    - בקובץ `client/src/app/features/contact/components/contact-form/`:
    - להחליף `mat-form-field` + `matInput` ב-`igds-input-field`
    - להחליף `mat-select` ב-`igds-dropdown`
    - להחליף `mat-datepicker` ב-`igds-date-picker`
    - להחליף כפתורים ב-`igds-button`
    - לשמור על Reactive Forms validation והודעות שגיאה בעברית
    - _דרישות: 13.1, 13.5, 5.1, 5.2, 5.3, 5.7, 5.8, 17.2_

  - [x] 5.3 מיגרציית contact-detail
    - בקובץ `client/src/app/features/contact/components/contact-detail/`:
    - להחליף `mat-card` ב-`igds-card` עם content projection (header, body, footer)
    - להחליף `mat-tab-group` ב-`igds-tabs`
    - להחליף כפתורים ב-`igds-button`
    - להחליף `matTooltip` ב-`igds-tooltip`
    - _דרישות: 13.1, 7.1, 7.2, 7.3, 4.1, 4.2, 4.3_

  - [x] 5.4 מיגרציית custom-fields ו-change-history
    - בקבצים `client/src/app/features/contact/components/custom-fields/` ו-`change-history/`:
    - להחליף רכיבי Material ברכיבי IGDS מקבילים (טפסים, טבלאות, כפתורים)
    - _דרישות: 13.1_

  - [x] 5.5 כתיבת בדיקות תכונה עבור מודול contact
    - **Property 2: שלמות ControlValueAccessor (round-trip)** — עבור שדות טופס ב-contact-form
    - **Property 4: שלמות פונקציונליות טבלאות** — עבור contact-list
    - **מאמת: דרישות 5.7, 6.2, 6.4, 17.3**

- [x] 6. פאזה 1 — ליבה עסקית: מודול candidacy
  - [x] 6.1 מיגרציית candidacy-list
    - בקובץ `client/src/app/features/candidacy/components/candidacy-list/`:
    - להחליף `mat-table` + `MatTableDataSource` ב-`igds-table` עם ניהול נתונים ידני
    - להחליף `mat-paginator` ב-`igds-pagination`
    - להחליף `mat-select` בפילטרים ב-`igds-dropdown`
    - להחליף `MatDialog` לאישור מחיקה ב-`igds-modal` / `IgdsModalService`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 13.2, 13.6, 6.1, 6.3, 8.1, 17.3_

  - [x] 6.2 מיגרציית candidacy-form ו-candidacy-detail
    - בקבצים `client/src/app/features/candidacy/components/candidacy-form/` ו-`candidacy-detail/`:
    - להחליף שדות טופס ב-`igds-input-field`, `igds-dropdown`, `igds-date-picker`, `igds-checkbox`
    - להחליף `mat-card` ב-`igds-card`, `mat-tab-group` ב-`igds-tabs`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 13.2, 5.1, 5.2, 5.3, 5.4, 7.1, 7.3_

  - [x] 6.3 מיגרציית status-timeline
    - בקובץ `client/src/app/features/candidacy/components/status-timeline/`:
    - להחליף רכיבי Material ב-`igds-step-indicator` ו-`igds-status-badge`
    - _דרישות: 13.2, 9.3, 9.4_

  - [x] 6.4 כתיבת בדיקת תכונה עבור מיפוי סטטוס מועמדות
    - **Property 8: מיפוי סטטוס מועמדות ל-status-badge**
    - **מאמת: דרישות 9.3**

- [x] 7. פאזה 1 — ליבה עסקית: מודול call-for-candidates
  - [x] 7.1 מיגרציית call-list
    - בקובץ `client/src/app/features/call-for-candidates/components/call-list/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 13.3, 6.1, 6.3_

  - [x] 7.2 מיגרציית call-form ו-call-detail
    - בקבצים `client/src/app/features/call-for-candidates/components/call-form/` ו-`call-detail/`:
    - להחליף שדות טופס ב-רכיבי IGDS (igds-input-field, igds-dropdown, igds-date-picker)
    - להחליף `mat-card` ב-`igds-card`, כפתורים ב-`igds-button`
    - להחליף `mat-chip` ב-`igds-tag`
    - _דרישות: 13.3, 5.1, 5.2, 5.3, 7.1, 8.3_

- [x] 8. נקודת ביקורת — פאזה 1
  - לוודא שכל הבדיקות עוברות. לשאול את המשתמש אם יש שאלות.


- [x] 9. פאזה 2 — תהליכים: מודול committee
  - [x] 9.1 מיגרציית committee-list
    - בקובץ `client/src/app/features/committee/components/committee-list/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 14.1, 6.1, 6.3_

  - [x] 9.2 מיגרציית committee-meeting
    - בקובץ `client/src/app/features/committee/components/committee-meeting/`:
    - להחליף `mat-card` ב-`igds-card`, שדות טופס ברכיבי IGDS
    - להחליף `mat-tab-group` ב-`igds-tabs`, כפתורים ב-`igds-button`
    - _דרישות: 14.1, 7.1, 7.3_

- [x] 10. פאזה 2 — תהליכים: מודול conflict
  - [x] 10.1 מיגרציית questionnaire-form
    - בקובץ `client/src/app/features/conflict/components/questionnaire-form/`:
    - להחליף שדות טופס ב-`igds-input-field`, `igds-radio-button`, `igds-checkbox`, `igds-dropdown`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 14.2, 5.1, 5.4, 5.5_

  - [x] 10.2 מיגרציית declarations-view ו-manual-review-list
    - בקבצים `client/src/app/features/conflict/components/declarations-view/` ו-`manual-review-list/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-card` ב-`igds-card`
    - להחליף `mat-accordion` ב-`igds-accordion`, כפתורים ב-`igds-button`
    - _דרישות: 14.2, 6.1, 7.1, 7.4_

- [x] 11. פאזה 2 — תהליכים: מודול dashboard
  - [x] 11.1 מיגרציית dashboard-view
    - בקובץ `client/src/app/features/dashboard/components/dashboard-view/`:
    - להחליף `mat-card` ב-`igds-card` בכרטיסי מדדים
    - להחליף `mat-table` ב-`igds-table` ברשימת פריטים הדורשים טיפול
    - להחליף `mat-chip-listbox` ב-`igds-tag` להצגת סטטוסים
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 14.3, 14.4, 14.5, 7.1, 8.3_

- [x] 12. נקודת ביקורת — פאזה 2
  - לוודא שכל הבדיקות עוברות. לשאול את המשתמש אם יש שאלות.

- [x] 13. פאזה 3 — תפעול: מודולי document, interview, exam
  - [x] 13.1 מיגרציית document-list
    - בקובץ `client/src/app/features/document/components/document-list/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 15.1, 15.5, 6.1, 6.3_

  - [x] 13.2 מיגרציית interview-list
    - בקובץ `client/src/app/features/interview/components/interview-list/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 15.2, 15.5, 6.1, 6.3_

  - [x] 13.3 מיגרציית exam-appeal-list
    - בקובץ `client/src/app/features/exam/components/exam-appeal-list/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 15.3, 15.5, 6.1, 6.3_

- [x] 14. פאזה 3 — תפעול: מודול notification
  - [x] 14.1 מיגרציית template-list ו-template-editor
    - בקבצים `client/src/app/features/notification/components/template-list/` ו-`template-editor/`:
    - להחליף `mat-table` ב-`igds-table` ב-template-list
    - להחליף שדות טופס ברכיבי IGDS ב-template-editor
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 15.4, 15.5, 6.1_

  - [x] 14.2 מיגרציית notification-log ו-send-notification
    - בקבצים `client/src/app/features/notification/components/notification-log/` ו-`send-notification/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination` ב-notification-log
    - להחליף שדות טופס ברכיבי IGDS ב-send-notification
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 15.4, 15.5, 5.1, 5.2, 6.1, 6.3_

  - [x] 14.3 כתיבת בדיקת תכונה עבור עימוד טבלאות
    - **Property 5: חישוב עימוד נכון**
    - **מאמת: דרישות 6.6**

- [x] 15. נקודת ביקורת — פאזה 3
  - לוודא שכל הבדיקות עוברות. לשאול את המשתמש אם יש שאלות.


- [x] 16. פאזה 4 — ניהול ותצורה: מודול admin/org-units
  - [x] 16.1 מיגרציית org-unit-list
    - בקובץ `client/src/app/features/admin/org-units/components/org-unit-list/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 16.1, 6.1, 6.3_

  - [x] 16.2 מיגרציית workflow-config, status-config ו-transition-config
    - בקבצים `client/src/app/features/admin/org-units/components/workflow-config/`, `status-config/`, `transition-config/`:
    - להחליף שדות טופס ברכיבי IGDS (igds-input-field, igds-dropdown, igds-toggle)
    - להחליף `mat-table` ב-`igds-table`, `mat-card` ב-`igds-card`
    - להחליף `mat-accordion` ב-`igds-accordion`, כפתורים ב-`igds-button`
    - _דרישות: 16.1, 5.1, 5.2, 5.6, 6.1, 7.1, 7.4_

- [x] 17. פאזה 4 — ניהול ותצורה: מודולי report ו-role
  - [x] 17.1 מיגרציית report-selector ו-report-results
    - בקבצים `client/src/app/features/report/components/report-selector/` ו-`report-results/`:
    - להחליף `mat-select` ב-`igds-dropdown`, `mat-datepicker` ב-`igds-date-picker` ב-report-selector
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination` ב-report-results
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 16.2, 5.2, 5.3, 6.1, 6.3_

  - [x] 17.2 מיגרציית role-list, role-form ו-user-assignment
    - בקבצים `client/src/app/features/role/components/role-list/`, `role-form/`, `user-assignment/`:
    - להחליף `mat-table` ב-`igds-table` ב-role-list ו-user-assignment
    - להחליף שדות טופס ברכיבי IGDS ב-role-form
    - להחליף `mat-checkbox` ב-`igds-checkbox` ב-user-assignment
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 16.3, 5.1, 5.4, 6.1_

- [x] 18. פאזה 4 — ניהול ותצורה: מודולי tenure, quota, org-structure, threshold-check
  - [x] 18.1 מיגרציית tenure-list ו-tenure-form
    - בקבצים `client/src/app/features/tenure/components/tenure-list/` ו-`tenure-form/`:
    - להחליף `mat-table` ב-`igds-table` ב-tenure-list
    - להחליף שדות טופס ברכיבי IGDS ב-tenure-form
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 16.4, 5.1, 5.2, 6.1_

  - [x] 18.2 מיגרציית quota-list
    - בקובץ `client/src/app/features/quota/components/quota-list/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-paginator` ב-`igds-pagination`
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 16.5, 6.1, 6.3_

  - [x] 18.3 מיגרציית org-tree
    - בקובץ `client/src/app/features/org-structure/components/org-tree/`:
    - להחליף רכיבי Material ברכיבי IGDS (igds-card, igds-accordion, igds-button)
    - _דרישות: 16.6_

  - [x] 18.4 מיגרציית threshold-results
    - בקובץ `client/src/app/features/threshold-check/components/threshold-results/`:
    - להחליף `mat-table` ב-`igds-table`, `mat-card` ב-`igds-card`
    - להחליף `igds-status-badge` להצגת תוצאות סף
    - להחליף כפתורים ב-`igds-button`
    - _דרישות: 16.7, 6.1, 7.1, 9.3_

- [x] 19. נקודת ביקורת — פאזה 4
  - לוודא שכל הבדיקות עוברות. לשאול את המשתמש אם יש שאלות.

- [x] 20. בדיקות תכונה חוצות-פאזות (RTL, נגישות, Design Tokens)
  - [x] 20.1 כתיבת בדיקת תכונה עבור RTL ו-CSS logical properties
    - **Property 9: תמיכה ב-RTL ו-CSS logical properties**
    - **מאמת: דרישות 10.1, 10.2**

  - [x] 20.2 כתיבת בדיקת תכונה עבור שימוש בלעדי ב-Design Tokens
    - **Property 10: שימוש בלעדי ב-Design Tokens**
    - **מאמת: דרישות 10.3, 10.5**

  - [x] 20.3 כתיבת בדיקת תכונה עבור תכונות ARIA
    - **Property 11: תכונות ARIA ברכיבים אינטראקטיביים**
    - **מאמת: דרישות 11.2, 11.5**

  - [x] 20.4 כתיבת בדיקת תכונה עבור גודל מגע ו-focus-visible
    - **Property 12: גודל מינימלי למגע ו-focus-visible**
    - **מאמת: דרישות 11.3, 11.4**

  - [x] 20.5 כתיבת בדיקת תכונה עבור אין שרידי Material
    - **Property 1: אין שרידי Angular Material בתבניות שהומרו**
    - **מאמת: דרישות 4.1-4.3, 5.1-5.6, 6.1, 6.3, 7.1, 7.3-7.4, 8.1-8.3, 9.1-9.2**

  - [x] 20.6 כתיבת בדיקת תכונה עבור שמירה על מאפייני כפתורים
    - **Property 15: שמירה על מאפייני כפתורים**
    - **מאמת: דרישות 4.4**

  - [x] 20.7 כתיבת בדיקת תכונה עבור הודעות שגיאה וולידציה
    - **Property 3: שימור הודעות שגיאה וולידציה בטפסים**
    - **מאמת: דרישות 5.8, 17.2**

- [x] 21. ניקוי — הסרת תלויות Angular Material
  - [x] 21.1 הסרת Angular Material מ-SharedModule
    - בקובץ `client/src/app/shared/shared.module.ts`:
    - להסיר את כל מודולי Material מ-`MATERIAL_MODULES` ואת כל ה-imports שלהם
    - לוודא ש-`IgdsModule` נשאר מיוצא
    - _דרישות: 1.4, 12.1_

  - [x] 21.2 הסרת Angular Material מ-CoreModule
    - בקובץ `client/src/app/core/core.module.ts`:
    - להסיר ייבואי MatToolbarModule, MatSidenavModule, MatListModule, MatMenuModule, MatDividerModule
    - _דרישות: 12.2_

  - [x] 21.3 הסרת חבילות Material מ-package.json
    - בקובץ `client/package.json`:
    - להסיר `@angular/material` ו-`@angular/cdk` מ-dependencies
    - _דרישות: 12.3_

  - [x] 21.4 ניקוי angular.json
    - בקובץ `client/angular.json`:
    - להסיר ייבוא theme של Angular Material (אם קיים ב-styles)
    - _דרישות: 12.4_

  - [x] 21.5 ניקוי styles.scss
    - בקובץ `client/src/styles.scss`:
    - להסיר `@use '@angular/material' as mat`
    - להסיר סגנונות `.mat-mdc-snack-bar-container` וכל סגנונות Material אחרים
    - _דרישות: 12.5_

  - [x] 21.6 ניקוי index.html
    - בקובץ `client/src/index.html`:
    - להסיר ייבוא Material Icons מ-Google Fonts (אם אינו נדרש עוד)
    - להסיר `class="mat-typography"` מ-body
    - _דרישות: 12.6_

- [x] 22. נקודת ביקורת סופית — סיום מיגרציה
  - לוודא שכל הבדיקות עוברות. לשאול את המשתמש אם יש שאלות.

- [x] 23. משימה עתידית — מעבר לחבילות IGDS רשמיות
  - כאשר npm יהיה זמין במכונה, לשקול מעבר מהרכיבים המותאמים ב-`shared/igds/` לחבילות הרשמיות: `@igds/angular`, `@igds/tokens`, `@igds/core-web`
  - להתקין חבילות: `npm install @igds/angular @igds/tokens @igds/core-web`
  - להחליף ייבואים מ-`shared/igds/` לייבואים מ-`@igds/angular`
  - להחליף `igds-tokens.scss` ב-tokens מ-`@igds/tokens`
  - _הערה: משימה זו תלויה בהתקנת npm/Node.js על המכונה_

## הערות

- משימות המסומנות ב-`*` הן אופציונליות וניתן לדלג עליהן לטובת MVP מהיר יותר
- כל משימה מפנה לדרישות ספציפיות לצורך מעקב
- נקודות ביקורת מבטיחות אימות הדרגתי
- בדיקות תכונה מאמתות נכונות כללית, בדיקות יחידה מאמתות דוגמאות ספציפיות ו-edge cases
- במהלך המיגרציה, SharedModule מייצא הן Material והן IGDS במקביל — הסרת Material רק בשלב 21
