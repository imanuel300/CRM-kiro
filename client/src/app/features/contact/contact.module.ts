import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { ContactListComponent } from './components/contact-list/contact-list.component';
import { ContactFormComponent } from './components/contact-form/contact-form.component';
import { ContactDetailComponent } from './components/contact-detail/contact-detail.component';
import { ChangeHistoryComponent } from './components/change-history/change-history.component';
import { CustomFieldsComponent } from './components/custom-fields/custom-fields.component';

const routes: Routes = [
  { path: '', component: ContactListComponent },
  { path: 'new', component: ContactFormComponent },
  { path: ':id', component: ContactDetailComponent },
  { path: ':id/edit', component: ContactFormComponent },
];

@NgModule({
  declarations: [
    ContactListComponent,
    ContactFormComponent,
    ContactDetailComponent,
    ChangeHistoryComponent,
    CustomFieldsComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class ContactModule {}
