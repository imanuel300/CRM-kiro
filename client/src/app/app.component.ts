import { Component, OnInit, OnDestroy } from '@angular/core';
import { Router } from '@angular/router';
import { Store } from '@ngrx/store';
import { Subject, takeUntil } from 'rxjs';
import { IdleService } from './core/services/idle.service';
import { AuthActions } from './core/store/auth/auth.actions';
import { selectIsAuthenticated } from './core/store/auth/auth.selectors';
import { AppState } from './core/store/app.state';

@Component({
  selector: 'app-root',
  template: `
    <ng-container *ngIf="isAuthenticated; else loginView">
      <app-layout>
        <router-outlet></router-outlet>
      </app-layout>
    </ng-container>
    <ng-template #loginView>
      <router-outlet></router-outlet>
    </ng-template>
  `,
})
export class AppComponent implements OnInit, OnDestroy {
  isAuthenticated = true;
  private destroy$ = new Subject<void>();

  constructor(
    private store: Store<AppState>,
    private idleService: IdleService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.store
      .select(selectIsAuthenticated)
      .pipe(takeUntil(this.destroy$))
      .subscribe((auth) => {
        this.isAuthenticated = auth;
        if (auth) {
          this.idleService.startWatching();
        } else {
          this.idleService.stopWatching();
        }
      });

    this.idleService.onTimeout$.pipe(takeUntil(this.destroy$)).subscribe(() => {
      this.store.dispatch(AuthActions.logout());
      this.router.navigate(['/login']);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
