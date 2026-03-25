import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';

import { ConfirmDialogComponent } from './components/confirm-dialog/confirm-dialog.component';
import { LoadingSpinnerComponent } from './components/loading-spinner/loading-spinner.component';
import { HasPermissionDirective } from './directives/has-permission.directive';
import { HebrewDatePipe } from './pipes/hebrew-date.pipe';
import { IgdsModule } from '@igds/angular';

@NgModule({
  declarations: [
    ConfirmDialogComponent,
    LoadingSpinnerComponent,
    HasPermissionDirective,
    HebrewDatePipe,
  ],
  imports: [CommonModule, FormsModule, ReactiveFormsModule, RouterModule, IgdsModule],
  exports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    IgdsModule,
    ConfirmDialogComponent,
    LoadingSpinnerComponent,
    HasPermissionDirective,
    HebrewDatePipe,
  ],
})
export class SharedModule {}
