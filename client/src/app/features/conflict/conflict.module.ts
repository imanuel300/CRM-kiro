import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { DeclarationsViewComponent } from './components/declarations-view/declarations-view.component';
import { QuestionnaireFormComponent } from './components/questionnaire-form/questionnaire-form.component';
import { ManualReviewListComponent } from './components/manual-review-list/manual-review-list.component';

const routes: Routes = [
  { path: '', redirectTo: 'manual-review', pathMatch: 'full' },
  { path: 'manual-review', component: ManualReviewListComponent },
  { path: 'candidacy/:candidacyId', component: DeclarationsViewComponent },
  { path: 'candidacy/:candidacyId/questionnaire/new', component: QuestionnaireFormComponent },
  { path: 'candidacy/:candidacyId/questionnaire/:id', component: QuestionnaireFormComponent },
];

@NgModule({
  declarations: [
    DeclarationsViewComponent,
    QuestionnaireFormComponent,
    ManualReviewListComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class ConflictModule {}
