import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { ThresholdResultsComponent } from './components/threshold-results/threshold-results.component';
import { ManualCheckFormComponent } from './components/manual-check-form/manual-check-form.component';

const routes: Routes = [
  { path: 'candidacy/:candidacyId', component: ThresholdResultsComponent },
  { path: 'candidacy/:candidacyId/manual-check', component: ManualCheckFormComponent },
  { path: 'candidacy/:candidacyId/manual-check/:conditionId', component: ManualCheckFormComponent },
];

@NgModule({
  declarations: [ThresholdResultsComponent, ManualCheckFormComponent],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class ThresholdCheckModule {}
