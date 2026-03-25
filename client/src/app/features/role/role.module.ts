import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { RoleListComponent } from './components/role-list/role-list.component';
import { RoleFormComponent } from './components/role-form/role-form.component';
import { UserAssignmentComponent } from './components/user-assignment/user-assignment.component';

const routes: Routes = [
  { path: '', component: RoleListComponent },
  { path: 'new', component: RoleFormComponent },
  { path: ':id', component: RoleFormComponent },
  { path: ':id/assign', component: UserAssignmentComponent },
];

@NgModule({
  declarations: [
    RoleListComponent,
    RoleFormComponent,
    UserAssignmentComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class RoleModule {}
