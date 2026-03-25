import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { CandidacyListComponent } from './components/candidacy-list/candidacy-list.component';
import { CandidacyFormComponent } from './components/candidacy-form/candidacy-form.component';
import { CandidacyDetailComponent } from './components/candidacy-detail/candidacy-detail.component';
import { StatusTimelineComponent } from './components/status-timeline/status-timeline.component';

const routes: Routes = [
  { path: '', component: CandidacyListComponent },
  { path: 'new', component: CandidacyFormComponent },
  { path: ':id', component: CandidacyDetailComponent },
];

@NgModule({
  declarations: [
    CandidacyListComponent,
    CandidacyFormComponent,
    CandidacyDetailComponent,
    StatusTimelineComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class CandidacyModule {}
