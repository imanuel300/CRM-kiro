# מסמך דרישות — מיגרציה מ-Angular Material ל-IGDS

## מבוא

מסמך זה מגדיר את הדרישות למיגרציה מלאה של ממשק המשתמש במערכת ניהול מועמדויות מרכיבי Angular Material לרכיבי IGDS (מערכת העיצוב הממשלתית הישראלית). המיגרציה תתבצע בשלבים (פאזות) כך שהמערכת תישאר פעילה ופריסה עצמאית תתאפשר בכל שלב. ספריית IGDS כבר קיימת ב-`client/src/app/shared/igds/` עם 22 רכיבים ודירקטיבה אחת.

## מילון מונחים

- **IGDS**: Israeli Government Design System — מערכת העיצוב הממשלתית הישראלית
- **Angular_Material**: ספריית רכיבי UI של Angular המבוססת על Material Design
- **SharedModule**: מודול Angular משותף המייצא רכיבים, דירקטיבות ומודולי Material לשימוש בכל המערכת
- **IgdsModule**: מודול Angular המכיל את כל רכיבי IGDS ומיוצא דרך SharedModule
- **Design_Tokens**: משתני CSS מוגדרים מראש (custom properties) המייצגים ערכי עיצוב (צבעים, ריווח, טיפוגרפיה) של IGDS
- **RTL**: Right-to-Left — כיוון כתיבה מימין לשמאל (עברית)
- **Feature_Module**: מודול Angular המכיל פיצ'ר עסקי שלם (לדוגמה: contact, candidacy)
- **CoreModule**: מודול Angular המכיל רכיבי ליבה כגון Layout, ניווט ושירותים גלובליים
- **ControlValueAccessor**: ממשק Angular המאפשר לרכיב מותאם אישית לעבוד עם Reactive Forms ו-ngModel
- **מיגרציה**: תהליך החלפת רכיבי Angular Material ברכיבי IGDS תוך שמירה על פונקציונליות קיימת
- **פאזה**: שלב מיגרציה עצמאי הכולל מספר Feature Modules שניתן לפרוס בנפרד

## דרישות

### דרישה 1: אסטרטגיית מיגרציה הדרגתית בפאזות

**סיפור משתמש:** כמפתח, אני רוצה שהמיגרציה תתבצע בשלבים מוגדרים, כדי שהמערכת תישאר יציבה ופעילה לאורך כל התהליך.

#### קריטריוני קבלה

1. THE מערכת_המיגרציה SHALL חלק את המיגרציה לחמש פאזות עצמאיות לפי סדר זה:
   - פאזה 0: תשתית (CoreModule, SharedModule — layout, confirm-dialog, loading-spinner, notification service)
   - פאזה 1: מודולי ליבה עסקיים (contact, candidacy, call-for-candidates)
   - פאזה 2: מודולי תהליך (committee, conflict, dashboard)
   - פאזה 3: מודולי תפעול (document, interview, exam, notification)
   - פאזה 4: מודולי ניהול ותצורה (admin/org-units, report, role, tenure, quota, org-structure, threshold-check)
2. WHEN פאזה מסוימת הושלמה, THE מערכת_המיגרציה SHALL אפשר פריסה עצמאית של הפאזה ללא תלות בפאזות הבאות
3. WHILE פאזה מסוימת בתהליך מיגרציה, THE SharedModule SHALL ייצא הן את מודולי Angular Material והן את IgdsModule במקביל
4. WHEN כל הרכיבים בכל הפאזות הומרו, THE SharedModule SHALL הסר את כל ייבואי Angular Material מהמערכת
5. THE מערכת_המיגרציה SHALL שמור על תאימות לאחור בין רכיבים שהומרו לרכיבים שטרם הומרו בכל פאזה

### דרישה 2: מיגרציית רכיב Layout ותשתית ניווט (פאזה 0)

**סיפור משתמש:** כמשתמש, אני רוצה שהתפריט הצדדי, סרגל הכלים העליון והניווט יעברו ל-IGDS, כדי שהמראה הכללי של המערכת יתאים למערכת העיצוב הממשלתית.

#### קריטריוני קבלה

1. WHEN המיגרציה של LayoutComponent מתבצעת, THE LayoutComponent SHALL החלף את mat-toolbar ברכיב header מותאם המשתמש ב-Design_Tokens של IGDS
2. WHEN המיגרציה של LayoutComponent מתבצעת, THE LayoutComponent SHALL החלף את mat-sidenav ו-mat-nav-list ברכיב igds-side-menu
3. WHEN המיגרציה של LayoutComponent מתבצעת, THE LayoutComponent SHALL החלף את mat-menu (תפריט משתמש) ברכיב igds-dropdown או igds-drawer
4. WHEN המיגרציה של BreadcrumbsComponent מתבצעת, THE BreadcrumbsComponent SHALL השתמש ברכיב igds-breadcrumbs
5. THE LayoutComponent SHALL שמור על כל פריטי הניווט הקיימים (17 פריטים) עם סינון לפי הרשאות
6. THE LayoutComponent SHALL תמוך בפתיחה וסגירה של התפריט הצדדי

### דרישה 3: מיגרציית רכיבי Shared (פאזה 0)

**סיפור משתמש:** כמפתח, אני רוצה שרכיבי ה-Shared המשותפים (confirm-dialog, loading-spinner, notification) יעברו ל-IGDS, כדי שכל הרכיבים המשותפים ישתמשו בעיצוב אחיד.

#### קריטריוני קבלה

1. WHEN המיגרציה של ConfirmDialogComponent מתבצעת, THE ConfirmDialogComponent SHALL החלף את MatDialog ו-mat-dialog-* ברכיב igds-modal
2. WHEN המיגרציה של ConfirmDialogComponent מתבצעת, THE ConfirmDialogComponent SHALL החלף את mat-button ו-mat-raised-button ברכיבי igds-button (variant secondary ו-primary בהתאמה)
3. WHEN המיגרציה של NotificationService מתבצעת, THE NotificationService SHALL החלף את MatSnackBar ברכיב igds-toast
4. WHEN המיגרציה של LoadingSpinnerComponent מתבצעת, THE LoadingSpinnerComponent SHALL החלף את mat-progress-spinner ברכיב igds-progress-bar או באנימציית טעינה מותאמת בהתאם ל-Design_Tokens
5. THE ConfirmDialogComponent SHALL שמור על ממשק ConfirmDialogData הקיים (title, message, confirmText, cancelText)

### דרישה 4: מיגרציית כפתורים

**סיפור משתמש:** כמפתח, אני רוצה שכל הכפתורים במערכת יוחלפו ברכיב igds-button, כדי שהמראה יהיה אחיד ותואם IGDS.

#### קריטריוני קבלה

1. WHEN רכיב מכיל mat-button, THE מיגרציה SHALL החלף אותו ברכיב igds-button עם variant="secondary"
2. WHEN רכיב מכיל mat-raised-button או mat-flat-button עם color="primary", THE מיגרציה SHALL החלף אותו ברכיב igds-button עם variant="primary"
3. WHEN רכיב מכיל mat-icon-button, THE מיגרציה SHALL החלף אותו ברכיב igds-button עם iconOnly=true
4. THE igds-button SHALL שמור על כל מאפייני disabled, type ו-aria-label הקיימים בכפתור המקורי
5. WHEN כפתור מכיל mat-icon כתוכן, THE מיגרציה SHALL שמור על האייקון באמצעות content projection של igds-button

### דרישה 5: מיגרציית שדות טופס

**סיפור משתמש:** כמפתח, אני רוצה שכל שדות הטופס (input, select, datepicker, checkbox, radio, toggle) יוחלפו ברכיבי IGDS, כדי שכל הטפסים יתאימו למערכת העיצוב הממשלתית.

#### קריטריוני קבלה

1. WHEN רכיב מכיל mat-form-field עם mat-input, THE מיגרציה SHALL החלף אותו ברכיב igds-input-field עם label, formControlName ו-error messages תואמים
2. WHEN רכיב מכיל mat-select, THE מיגרציה SHALL החלף אותו ברכיב igds-dropdown עם אותן אפשרויות בחירה
3. WHEN רכיב מכיל mat-datepicker, THE מיגרציה SHALL החלף אותו ברכיב igds-date-picker
4. WHEN רכיב מכיל mat-checkbox, THE מיגרציה SHALL החלף אותו ברכיב igds-checkbox
5. WHEN רכיב מכיל mat-radio-group ו-mat-radio-button, THE מיגרציה SHALL החלף אותם ברכיב igds-radio-button
6. WHEN רכיב מכיל mat-slide-toggle, THE מיגרציה SHALL החלף אותו ברכיב igds-toggle
7. THE רכיבי_IGDS_טפסיים SHALL תמכו ב-Reactive Forms באמצעות ControlValueAccessor (formControlName ו-formControl)
8. THE רכיבי_IGDS_טפסיים SHALL הציגו הודעות שגיאה (validation errors) בעברית כפי שהוצגו ברכיבי Material המקוריים

### דרישה 6: מיגרציית טבלאות ועימוד

**סיפור משתמש:** כמפתח, אני רוצה שכל הטבלאות ורכיבי העימוד יוחלפו ברכיבי IGDS, כדי שתצוגת הנתונים תתאים למערכת העיצוב הממשלתית.

#### קריטריוני קבלה

1. WHEN רכיב מכיל mat-table, THE מיגרציה SHALL החלף אותו ברכיב igds-table
2. WHEN רכיב מכיל mat-sort ו-mat-sort-header, THE מיגרציה SHALL העבר את יכולת המיון לרכיב igds-table
3. WHEN רכיב מכיל mat-paginator, THE מיגרציה SHALL החלף אותו ברכיב igds-pagination
4. THE igds-table SHALL תמוך בהגדרת עמודות, תצוגת כותרות בעברית ותבנית מותאמת לתאים
5. THE igds-table SHALL הציג הודעת "לא נמצאו תוצאות" כאשר אין נתונים
6. THE igds-pagination SHALL תמוך באפשרויות גודל עמוד (10, 25, 50) וניווט לעמוד ראשון ואחרון

### דרישה 7: מיגרציית כרטיסים, טאבים ואקורדיון

**סיפור משתמש:** כמפתח, אני רוצה שכל רכיבי הפריסה (cards, tabs, accordion) יוחלפו ברכיבי IGDS, כדי שמבנה העמודים יתאים למערכת העיצוב הממשלתית.

#### קריטריוני קבלה

1. WHEN רכיב מכיל mat-card, THE מיגרציה SHALL החלף אותו ברכיב igds-card
2. WHEN רכיב מכיל mat-card-header, mat-card-title ו-mat-card-content, THE מיגרציה SHALL מפה אותם לאזורי content projection מתאימים ב-igds-card
3. WHEN רכיב מכיל mat-tab-group ו-mat-tab, THE מיגרציה SHALL החלף אותם ברכיב igds-tabs
4. WHEN רכיב מכיל mat-accordion ו-mat-expansion-panel, THE מיגרציה SHALL החלף אותם ברכיב igds-accordion
5. THE igds-card SHALL תמוך בכותרת, תוכן ופעולות (actions) כפי שנתמכו ב-mat-card

### דרישה 8: מיגרציית דיאלוגים, הודעות וטולטיפים

**סיפור משתמש:** כמפתח, אני רוצה שכל הדיאלוגים, ההודעות והטולטיפים יוחלפו ברכיבי IGDS, כדי שכל האינטראקציות עם המשתמש יתאימו למערכת העיצוב הממשלתית.

#### קריטריוני קבלה

1. WHEN רכיב משתמש ב-MatDialog.open(), THE מיגרציה SHALL החלף את הקריאה בשימוש ברכיב igds-modal
2. WHEN רכיב משתמש ב-matTooltip, THE מיגרציה SHALL החלף אותו בדירקטיבת igds-tooltip
3. WHEN רכיב מכיל mat-chip או mat-chip-listbox, THE מיגרציה SHALL החלף אותם ברכיב igds-tag
4. THE igds-modal SHALL תמוך בפתיחה וסגירה פרוגרמטית ובהחזרת ערך בעת סגירה (בדומה ל-MatDialogRef.afterClosed)
5. THE igds-toast SHALL תמוך בסוגי הודעות: success, error, warning ו-info

### דרישה 9: מיגרציית רכיבי התקדמות ומצב

**סיפור משתמש:** כמפתח, אני רוצה שכל רכיבי ההתקדמות והמצב יוחלפו ברכיבי IGDS, כדי שהמשתמש יקבל משוב חזותי עקבי.

#### קריטריוני קבלה

1. WHEN רכיב מכיל mat-progress-bar, THE מיגרציה SHALL החלף אותו ברכיב igds-progress-bar
2. WHEN רכיב מכיל mat-progress-spinner, THE מיגרציה SHALL החלף אותו ברכיב igds-progress-bar או באנימציית טעינה מותאמת
3. WHEN רכיב מציג סטטוס מועמדות, THE מיגרציה SHALL השתמש ברכיב igds-status-badge
4. THE igds-step-indicator SHALL שמש להצגת שלבי תהליך (כגון status-timeline במועמדויות)


### דרישה 10: תמיכה ב-RTL ועיצוב עברי

**סיפור משתמש:** כמשתמש דובר עברית, אני רוצה שכל הרכיבים יתמכו בכיוון RTL ובטיפוגרפיה עברית, כדי שהממשק יהיה נוח ותקין לשימוש בעברית.

#### קריטריוני קבלה

1. THE כל_רכיבי_IGDS_המומרים SHALL ירשו את כיוון ה-RTL מאלמנט ה-html (dir="rtl")
2. THE כל_רכיבי_IGDS_המומרים SHALL השתמשו ב-CSS logical properties (margin-inline-start, padding-inline-end וכו') במקום כיוונים פיזיים (left, right)
3. THE כל_רכיבי_IGDS_המומרים SHALL השתמשו בגופן Heebo כגופן ראשי בהתאם ל-Design_Token של --igds-font-family
4. WHEN תפריט צדדי מוצג, THE igds-side-menu SHALL יופיע בצד ימין של המסך (position="end" ב-RTL)
5. THE כל_רכיבי_IGDS_המומרים SHALL לא ישתמשו בערכי צבע, ריווח או טיפוגרפיה מקודדים (hardcoded) אלא רק ב-Design_Tokens

### דרישה 11: נגישות (Accessibility)

**סיפור משתמש:** כמשתמש עם מוגבלות, אני רוצה שכל הרכיבים המומרים ישמרו על רמת נגישות גבוהה, כדי שאוכל להשתמש במערכת באמצעות מקלדת וקורא מסך.

#### קריטריוני קבלה

1. THE כל_רכיבי_IGDS_המומרים SHALL תמכו בניווט מלא באמצעות מקלדת (Tab, Enter, Escape, חצים)
2. THE כל_רכיבי_IGDS_המומרים SHALL כללו תכונות ARIA מתאימות (aria-label, aria-expanded, aria-selected, role)
3. THE כל_האלמנטים_האינטראקטיביים SHALL יהיו בגודל מינימלי של 44px למגע (touch target)
4. THE כל_רכיבי_IGDS_המומרים SHALL יציגו מצב focus-visible באמצעות --igds-border-focused
5. IF רכיב Material מקורי כלל תכונות ARIA, THEN THE רכיב_IGDS_המחליף SHALL שמור על תכונות ARIA אלו או יספק תכונות שוות ערך

### דרישה 12: הסרת תלויות Angular Material

**סיפור משתמש:** כמפתח, אני רוצה שבסיום המיגרציה כל תלויות Angular Material יוסרו מהפרויקט, כדי להקטין את גודל ה-bundle ולמנוע בלבול.

#### קריטריוני קבלה

1. WHEN כל הפאזות הושלמו, THE SharedModule SHALL הסר את כל מודולי Angular Material מרשימת MATERIAL_MODULES
2. WHEN כל הפאזות הושלמו, THE CoreModule SHALL הסר את כל ייבואי Angular Material (MatToolbarModule, MatSidenavModule, MatListModule, MatMenuModule, MatDividerModule)
3. WHEN כל הפאזות הושלמו, THE package.json SHALL הסר את חבילות @angular/material ו-@angular/cdk מ-dependencies
4. WHEN כל הפאזות הושלמו, THE angular.json SHALL הסר את ייבוא ה-theme של Angular Material (אם קיים)
5. WHEN כל הפאזות הושלמו, THE styles.scss SHALL הסר את `@use '@angular/material' as mat` ואת כל הסגנונות הספציפיים ל-Material (כגון .mat-mdc-snack-bar-container)
6. WHEN כל הפאזות הושלמו, THE index.html SHALL הסר את ייבוא Material Icons מ-Google Fonts (אם אינו נדרש עוד) ואת class="mat-typography" מ-body

### דרישה 13: מיגרציית מודולי ליבה עסקיים (פאזה 1)

**סיפור משתמש:** כמפתח, אני רוצה שמודולי הליבה העסקיים (contact, candidacy, call-for-candidates) יומרו ל-IGDS, כדי שהפיצ'רים המרכזיים ביותר יעברו ראשונים.

#### קריטריוני קבלה

1. WHEN מיגרציית מודול contact מתבצעת, THE מיגרציה SHALL המר את כל חמשת הרכיבים: contact-list, contact-form, contact-detail, custom-fields, change-history
2. WHEN מיגרציית מודול candidacy מתבצעת, THE מיגרציה SHALL המר את כל ארבעת הרכיבים: candidacy-list, candidacy-form, candidacy-detail, status-timeline
3. WHEN מיגרציית מודול call-for-candidates מתבצעת, THE מיגרציה SHALL המר את כל שלושת הרכיבים: call-list, call-form, call-detail
4. WHEN contact-list מומר, THE igds-table SHALL החלף את mat-table עם תמיכה במיון ועימוד, ו-igds-search-field SHALL החלף את שדה החיפוש
5. WHEN contact-form מומר, THE igds-input-field, igds-dropdown ו-igds-date-picker SHALL החליפו את כל שדות הטופס תוך שמירה על Reactive Forms validation
6. WHEN candidacy-list מומר, THE igds-table SHALL החלף את mat-table, igds-dropdown SHALL החלף את mat-select בפילטרים, ו-igds-modal SHALL החלף את MatDialog לאישור מחיקה

### דרישה 14: מיגרציית מודולי תהליך (פאזה 2)

**סיפור משתמש:** כמפתח, אני רוצה שמודולי התהליך (committee, conflict, dashboard) יומרו ל-IGDS, כדי שתהליכי העבודה המרכזיים יתאימו לעיצוב הממשלתי.

#### קריטריוני קבלה

1. WHEN מיגרציית מודול committee מתבצעת, THE מיגרציה SHALL המר את committee-list ו-committee-meeting
2. WHEN מיגרציית מודול conflict מתבצעת, THE מיגרציה SHALL המר את questionnaire-form, declarations-view ו-manual-review-list
3. WHEN מיגרציית מודול dashboard מתבצעת, THE מיגרציה SHALL המר את dashboard-view
4. WHEN dashboard-view מומר, THE igds-card SHALL החלף את mat-card בכרטיסי מדדים, ו-igds-table SHALL החלף את mat-table ברשימת פריטים הדורשים טיפול
5. WHEN dashboard-view מומר, THE igds-tag SHALL החלף את mat-chip-listbox להצגת סטטוסים

### דרישה 15: מיגרציית מודולי תפעול (פאזה 3)

**סיפור משתמש:** כמפתח, אני רוצה שמודולי התפעול (document, interview, exam, notification) יומרו ל-IGDS.

#### קריטריוני קבלה

1. WHEN מיגרציית מודול document מתבצעת, THE מיגרציה SHALL המר את document-list
2. WHEN מיגרציית מודול interview מתבצעת, THE מיגרציה SHALL המר את interview-list
3. WHEN מיגרציית מודול exam מתבצעת, THE מיגרציה SHALL המר את exam-appeal-list
4. WHEN מיגרציית מודול notification מתבצעת, THE מיגרציה SHALL המר את כל ארבעת הרכיבים: template-list, template-editor, notification-log, send-notification
5. THE כל_רכיבי_הרשימה_בפאזה_זו SHALL השתמשו ב-igds-table ו-igds-pagination להצגת נתונים טבלאיים

### דרישה 16: מיגרציית מודולי ניהול ותצורה (פאזה 4)

**סיפור משתמש:** כמפתח, אני רוצה שמודולי הניהול והתצורה יומרו ל-IGDS כפאזה אחרונה, כדי להשלים את המיגרציה.

#### קריטריוני קבלה

1. WHEN מיגרציית מודול admin/org-units מתבצעת, THE מיגרציה SHALL המר את org-unit-list, workflow-config, status-config ו-transition-config
2. WHEN מיגרציית מודול report מתבצעת, THE מיגרציה SHALL המר את report-selector ו-report-results
3. WHEN מיגרציית מודול role מתבצעת, THE מיגרציה SHALL המר את role-list, role-form ו-user-assignment
4. WHEN מיגרציית מודול tenure מתבצעת, THE מיגרציה SHALL המר את tenure-list ו-tenure-form
5. WHEN מיגרציית מודול quota מתבצעת, THE מיגרציה SHALL המר את quota-list
6. WHEN מיגרציית מודול org-structure מתבצעת, THE מיגרציה SHALL המר את org-tree
7. WHEN מיגרציית מודול threshold-check מתבצעת, THE מיגרציה SHALL המר את threshold-results

### דרישה 17: שמירה על פונקציונליות קיימת

**סיפור משתמש:** כמשתמש, אני רוצה שכל הפונקציונליות הקיימת תישמר לאחר המיגרציה, כדי שלא אאבד יכולות שהיו לי קודם.

#### קריטריוני קבלה

1. THE כל_רכיב_מומר SHALL שמור על אותה פונקציונליות עסקית שהייתה ברכיב Angular Material המקורי
2. THE כל_טופס_מומר SHALL שמור על כל כללי הוולידציה (Validators) הקיימים ועל הודעות השגיאה בעברית
3. THE כל_טבלה_מומרת SHALL שמור על יכולות מיון, עימוד וסינון שהיו קיימות
4. THE כל_דיאלוג_מומר SHALL שמור על זרימת הנתונים (פתיחה עם data, סגירה עם תוצאה)
5. THE ניווט_המערכת SHALL שמור על כל הנתיבים (routes) וסינון ההרשאות הקיימים
6. IF רכיב Material השתמש ב-MatTableDataSource, THEN THE רכיב_המומר SHALL ספק מנגנון מקביל לניהול נתוני הטבלה (סינון, מיון, עימוד)
