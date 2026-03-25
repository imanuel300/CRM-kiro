import { NgModule, Optional, SkipSelf } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { IgdsModule } from '@igds/angular';
import { LayoutComponent } from './layout/layout.component';
import { BreadcrumbsComponent } from './layout/breadcrumbs/breadcrumbs.component';
import { AuthService } from './services/auth.service';
import { ApiService } from './services/api.service';
import { NotificationService } from './services/notification.service';
import { IdleService } from './services/idle.service';
import { AuthGuard } from './guards/auth.guard';

@NgModule({
  declarations: [LayoutComponent, BreadcrumbsComponent],
  imports: [
    CommonModule,
    RouterModule,
    IgdsModule,
  ],
  exports: [LayoutComponent],
  providers: [AuthService, ApiService, NotificationService, IdleService, AuthGuard],
})
export class CoreModule {
  constructor(@Optional() @SkipSelf() parentModule: CoreModule) {
    if (parentModule) {
      throw new Error('CoreModule is already loaded. Import it in the AppModule only.');
    }
  }
}
