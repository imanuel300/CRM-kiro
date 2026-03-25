import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { DashboardViewComponent } from './components/dashboard-view/dashboard-view.component';

const routes: Routes = [
  { path: '', component: DashboardViewComponent },
];

@NgModule({
  declarations: [
    DashboardViewComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class DashboardModule {}
