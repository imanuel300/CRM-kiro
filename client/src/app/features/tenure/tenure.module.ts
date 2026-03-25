import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { TenureListComponent } from './components/tenure-list/tenure-list.component';
import { TenureFormComponent } from './components/tenure-form/tenure-form.component';
import { TenureAlertsComponent } from './components/tenure-alerts/tenure-alerts.component';

const routes: Routes = [
  { path: '', component: TenureListComponent },
  { path: 'new', component: TenureFormComponent },
  { path: 'alerts', component: TenureAlertsComponent },
  { path: ':id', component: TenureFormComponent },
  { path: ':id/end', component: TenureFormComponent },
];

@NgModule({
  declarations: [
    TenureListComponent,
    TenureFormComponent,
    TenureAlertsComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class TenureModule {}
