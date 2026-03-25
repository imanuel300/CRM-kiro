import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { CommitteeListComponent } from './components/committee-list/committee-list.component';
import { CommitteeMeetingComponent } from './components/committee-meeting/committee-meeting.component';

const routes: Routes = [
  { path: '', component: CommitteeListComponent },
  { path: ':committeeId/meetings/:meetingId', component: CommitteeMeetingComponent },
];

@NgModule({
  declarations: [
    CommitteeListComponent,
    CommitteeMeetingComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class CommitteeModule {}
