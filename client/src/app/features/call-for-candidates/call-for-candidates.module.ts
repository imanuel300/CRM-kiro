import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { CallListComponent } from './components/call-list/call-list.component';
import { CallFormComponent } from './components/call-form/call-form.component';
import { CallDetailComponent } from './components/call-detail/call-detail.component';

const routes: Routes = [
  { path: '', component: CallListComponent },
  { path: 'new', component: CallFormComponent },
  { path: ':id', component: CallDetailComponent },
  { path: ':id/edit', component: CallFormComponent },
];

@NgModule({
  declarations: [
    CallListComponent,
    CallFormComponent,
    CallDetailComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class CallForCandidatesModule {}
