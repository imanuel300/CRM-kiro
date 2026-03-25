import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/guards/auth.guard';

const routes: Routes = [
  {
    path: '',
    canActivate: [AuthGuard],
    data: { breadcrumb: 'ראשי' },
    children: [
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full',
      },
      {
        path: 'dashboard',
        loadChildren: () =>
          import('./features/dashboard/dashboard.module').then(
            (m) => m.DashboardModule
          ),
        data: { breadcrumb: 'לוח מחוונים' },
      },
      {
        path: 'candidacies',
        loadChildren: () =>
          import('./features/candidacy/candidacy.module').then(
            (m) => m.CandidacyModule
          ),
        data: { breadcrumb: 'מועמדויות' },
      },
      {
        path: 'contacts',
        loadChildren: () =>
          import('./features/contact/contact.module').then(
            (m) => m.ContactModule
          ),
        data: { breadcrumb: 'אנשי קשר' },
      },
      {
        path: 'calls',
        loadChildren: () =>
          import('./features/call-for-candidates/call-for-candidates.module').then(
            (m) => m.CallForCandidatesModule
          ),
        data: { breadcrumb: 'קולות קוראים' },
      },
      {
        path: 'exams',
        loadChildren: () =>
          import('./features/exam/exam.module').then((m) => m.ExamModule),
        data: { breadcrumb: 'מבחנים' },
      },
      {
        path: 'interviews',
        loadChildren: () =>
          import('./features/interview/interview.module').then(
            (m) => m.InterviewModule
          ),
        data: { breadcrumb: 'ראיונות' },
      },
      {
        path: 'committees',
        loadChildren: () =>
          import('./features/committee/committee.module').then(
            (m) => m.CommitteeModule
          ),
        data: { breadcrumb: 'ועדות' },
      },
      {
        path: 'documents',
        loadChildren: () =>
          import('./features/document/document.module').then(
            (m) => m.DocumentModule
          ),
        data: { breadcrumb: 'מסמכים' },
      },
      {
        path: 'notifications',
        loadChildren: () =>
          import('./features/notification/notification.module').then(
            (m) => m.NotificationModule
          ),
        data: { breadcrumb: 'דיוורים' },
      },
      {
        path: 'conflicts',
        loadChildren: () =>
          import('./features/conflict/conflict.module').then(
            (m) => m.ConflictModule
          ),
        data: { breadcrumb: 'ניגודי עניינים' },
      },
      {
        path: 'threshold-checks',
        loadChildren: () =>
          import('./features/threshold-check/threshold-check.module').then(
            (m) => m.ThresholdCheckModule
          ),
        data: { breadcrumb: 'תנאי סף' },
      },
      {
        path: 'roles',
        loadChildren: () =>
          import('./features/role/role.module').then(
            (m) => m.RoleModule
          ),
        data: { breadcrumb: 'הרשאות ותפקידים' },
      },
      {
        path: 'tenures',
        loadChildren: () =>
          import('./features/tenure/tenure.module').then(
            (m) => m.TenureModule
          ),
        data: { breadcrumb: 'כהונות' },
      },
      {
        path: 'quotas',
        loadChildren: () =>
          import('./features/quota/quota.module').then(
            (m) => m.QuotaModule
          ),
        data: { breadcrumb: 'מכסות' },
      },
      {
        path: 'org-structure',
        loadChildren: () =>
          import('./features/org-structure/org-structure.module').then(
            (m) => m.OrgStructureModule
          ),
        data: { breadcrumb: 'מבנה ארגוני' },
      },
      {
        path: 'reports',
        loadChildren: () =>
          import('./features/report/report.module').then(
            (m) => m.ReportModule
          ),
        data: { breadcrumb: 'דוחות' },
      },
      {
        path: 'admin',
        loadChildren: () =>
          import('./features/admin/admin.module').then((m) => m.AdminModule),
        data: { breadcrumb: 'הגדרות' },
      },
    ],
  },
  {
    path: 'login',
    loadChildren: () =>
      import('./features/login/login.module').then((m) => m.LoginModule),
  },
  { path: '**', redirectTo: '' },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule],
})
export class AppRoutingModule {}
