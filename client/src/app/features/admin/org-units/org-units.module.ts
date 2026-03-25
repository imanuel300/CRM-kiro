import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { OrgUnitListComponent } from './components/org-unit-list/org-unit-list.component';
import { OrgUnitFormComponent } from './components/org-unit-form/org-unit-form.component';
import { WorkflowConfigComponent } from './components/workflow-config/workflow-config.component';
import { StatusConfigComponent } from './components/status-config/status-config.component';
import { TransitionConfigComponent } from './components/transition-config/transition-config.component';

const routes: Routes = [
  { path: '', component: OrgUnitListComponent },
  { path: 'new', component: OrgUnitFormComponent },
  { path: ':id/edit', component: OrgUnitFormComponent },
  { path: ':id/workflow', component: WorkflowConfigComponent },
  { path: ':id/statuses', component: StatusConfigComponent },
  { path: ':id/transitions', component: TransitionConfigComponent },
];

@NgModule({
  declarations: [
    OrgUnitListComponent,
    OrgUnitFormComponent,
    WorkflowConfigComponent,
    StatusConfigComponent,
    TransitionConfigComponent,
  ],
  imports: [
    SharedModule,
    RouterModule.forChild(routes),
  ],
})
export class OrgUnitsModule {}
