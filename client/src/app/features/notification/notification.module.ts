import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { TemplateListComponent } from './components/template-list/template-list.component';
import { TemplateEditorComponent } from './components/template-editor/template-editor.component';
import { NotificationLogComponent } from './components/notification-log/notification-log.component';
import { SendNotificationComponent } from './components/send-notification/send-notification.component';

const routes: Routes = [
  { path: '', redirectTo: 'templates', pathMatch: 'full' },
  { path: 'templates', component: TemplateListComponent },
  { path: 'templates/new', component: TemplateEditorComponent },
  { path: 'templates/:id', component: TemplateEditorComponent },
  { path: 'logs', component: NotificationLogComponent },
  { path: 'send', component: SendNotificationComponent },
];

@NgModule({
  declarations: [
    TemplateListComponent,
    TemplateEditorComponent,
    NotificationLogComponent,
    SendNotificationComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class NotificationModule {}
