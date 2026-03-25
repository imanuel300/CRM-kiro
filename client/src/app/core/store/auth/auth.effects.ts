import { Injectable } from '@angular/core';
import { Actions, createEffect, ofType } from '@ngrx/effects';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { catchError, exhaustMap, map, tap } from 'rxjs/operators';
import { AuthActions } from './auth.actions';
import { AuthService } from '../../services/auth.service';

@Injectable()
export class AuthEffects {
  login$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.login),
      exhaustMap(({ username, password }) =>
        this.authService.login({ username, password }).pipe(
          map((response) =>
            AuthActions.loginSuccess({
              token: response.token!,
              requiresMfa: response.requiresMfa,
              userId: response.userId,
            })
          ),
          catchError((error) =>
            of(AuthActions.loginFailure({ error: error?.error?.message ?? 'שגיאה בהתחברות' }))
          )
        )
      )
    )
  );

  loginSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loginSuccess),
      map(({ requiresMfa }) => {
        if (!requiresMfa) {
          return AuthActions.loadProfile();
        }
        return { type: '[Auth] MFA Required - No Op' };
      })
    )
  );

  verifyMfa$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.verifyMfa),
      exhaustMap(({ userId, code }) =>
        this.authService.verifyMfa({ userId, code }).pipe(
          map((token) => AuthActions.mfaSuccess({ token })),
          catchError((error) =>
            of(AuthActions.mfaFailure({ error: error?.error?.message ?? 'קוד אימות שגוי' }))
          )
        )
      )
    )
  );

  mfaSuccess$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.mfaSuccess),
      map(() => AuthActions.loadProfile())
    )
  );

  loadProfile$ = createEffect(() =>
    this.actions$.pipe(
      ofType(AuthActions.loadProfile),
      exhaustMap(() =>
        this.authService.loadUserProfile().pipe(
          map((user) => AuthActions.loadProfileSuccess({ user })),
          catchError((error) =>
            of(AuthActions.loadProfileFailure({ error: error?.error?.message ?? 'שגיאה בטעינת פרופיל' }))
          )
        )
      )
    )
  );

  navigateAfterProfile$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.loadProfileSuccess),
        tap(() => this.router.navigate(['/dashboard']))
      ),
    { dispatch: false }
  );

  logout$ = createEffect(
    () =>
      this.actions$.pipe(
        ofType(AuthActions.logout),
        tap(() => {
          this.authService.logout();
          this.router.navigate(['/login']);
        })
      ),
    { dispatch: false }
  );

  constructor(
    private actions$: Actions,
    private authService: AuthService,
    private router: Router
  ) {}
}
