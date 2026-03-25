import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { SharedModule } from '@shared/shared.module';

import { DocumentListComponent } from './components/document-list/document-list.component';
import { DocumentUploadComponent } from './components/document-upload/document-upload.component';

const routes: Routes = [
  { path: '', component: DocumentListComponent },
  { path: 'upload', component: DocumentUploadComponent },
];

@NgModule({
  declarations: [
    DocumentListComponent,
    DocumentUploadComponent,
  ],
  imports: [SharedModule, RouterModule.forChild(routes)],
})
export class DocumentModule {}
