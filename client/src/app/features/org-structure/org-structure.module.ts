import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { OrgTreeComponent } from './components/org-tree/org-tree.component';
import { PositionAssignmentComponent } from './components/position-assignment/position-assignment.component';
import { OccupancyViewComponent } from './components/occupancy-view/occupancy-view.component';

const routes: Routes = [
  { path: '', component: OrgTreeComponent },
  { path: 'assign', component: PositionAssignmentComponent },
  { path: 'occupancy', component: OccupancyViewComponent },
];

@NgModule({
  declarations: [
    OrgTreeComponent,
    PositionAssignmentComponent,
    OccupancyViewComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class OrgStructureModule {}
