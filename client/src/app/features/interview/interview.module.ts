import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { InterviewListComponent } from './components/interview-list/interview-list.component';
import { InterviewFeedbackFormComponent } from './components/interview-feedback-form/interview-feedback-form.component';
import { InterviewScheduleComponent } from './components/interview-schedule/interview-schedule.component';

const routes: Routes = [
  { path: '', component: InterviewListComponent },
  { path: 'schedule', component: InterviewScheduleComponent },
  { path: ':id/feedback', component: InterviewFeedbackFormComponent },
];

@NgModule({
  declarations: [
    InterviewListComponent,
    InterviewFeedbackFormComponent,
    InterviewScheduleComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class InterviewModule {}
