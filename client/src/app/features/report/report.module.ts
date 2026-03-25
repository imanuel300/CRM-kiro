import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { ReportSelectorComponent } from './components/report-selector/report-selector.component';
import { ReportResultsComponent } from './components/report-results/report-results.component';

const routes: Routes = [
  { path: '', component: ReportSelectorComponent },
];

@NgModule({
  declarations: [
    ReportSelectorComponent,
    ReportResultsComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class ReportModule {}
