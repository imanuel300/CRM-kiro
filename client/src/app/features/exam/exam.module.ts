import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { ExamListComponent } from './components/exam-list/exam-list.component';
import { ExamScoreFormComponent } from './components/exam-score-form/exam-score-form.component';
import { ExamAppealListComponent } from './components/exam-appeal-list/exam-appeal-list.component';

const routes: Routes = [
  { path: '', component: ExamListComponent },
  { path: ':id/scores', component: ExamScoreFormComponent },
  { path: ':id/appeals', component: ExamAppealListComponent },
];

@NgModule({
  declarations: [
    ExamListComponent,
    ExamScoreFormComponent,
    ExamAppealListComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class ExamModule {}
