import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { QuotaListComponent } from './components/quota-list/quota-list.component';
import { QuotaFulfillmentComponent } from './components/quota-fulfillment/quota-fulfillment.component';
import { CandidacyAssignmentComponent } from './components/candidacy-assignment/candidacy-assignment.component';

const routes: Routes = [
  { path: '', component: QuotaListComponent },
  { path: 'fulfillment', component: QuotaFulfillmentComponent },
  { path: 'assign', component: CandidacyAssignmentComponent },
];

@NgModule({
  declarations: [
    QuotaListComponent,
    QuotaFulfillmentComponent,
    CandidacyAssignmentComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class QuotaModule {}
