import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

const routes: Routes = [
  {
    path: 'org-units',
    loadChildren: () =>
      import('./org-units/org-units.module').then((m) => m.OrgUnitsModule),
  },
  {
    path: '',
    redirectTo: 'org-units',
    pathMatch: 'full',
  },
];

@NgModule({
  declarations: [],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class AdminModule {}
