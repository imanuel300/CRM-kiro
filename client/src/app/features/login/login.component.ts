import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Store } from '@ngrx/store';
import { Subject, takeUntil } from 'rxjs';
import { AuthActions } from '@core/store/auth/auth.actions';
import {
  selectAuthError,
  selectAuthLoading,
  selectMfaRequired,
  selectMfaUserId,
} from '@core/store/auth/auth.selectors';
import { AppState } from '@core/store/app.state';

@Component({
  selector: 'app-login',
  template: `
    <div class="login-container">
      <igds-card class="login-card">
        <div igds-card-header>
          <h1 class="login-title">כניסה למערכת ניהול מועמדויות</h1>
        </div>
        <div igds-card-body>
          <!-- Login Form -->
          <form *ngIf="!mfaRequired" [formGroup]="loginForm" (ngSubmit)="onLogin()">
            <igds-input-field
              label="שם משתמש"
              formControlName="username"
              [required]="true">
            </igds-input-field>
            <igds-input-field
              label="סיסמה"
              type="password"
              formControlName="password"
              [required]="true">
            </igds-input-field>
            <igds-button variant="primary" type="submit"
                         [disabled]="loginForm.invalid || loading"
                         class="full-width">
              <app-loading-spinner *ngIf="loading" [loading]="true"></app-loading-spinner>
              <span *ngIf="!loading">כניסה</span>
            </igds-button>
          </form>

          <!-- MFA Form -->
          <form *ngIf="mfaRequired" [formGroup]="mfaForm" (ngSubmit)="onVerifyMfa()">
            <p class="mfa-text">נא להזין את קוד האימות שנשלח אליך</p>
            <igds-input-field
              label="קוד אימות"
              formControlName="code"
              [required]="true">
            </igds-input-field>
            <igds-button variant="primary" type="submit"
                         [disabled]="mfaForm.invalid || loading"
                         class="full-width">
              <app-loading-spinner *ngIf="loading" [loading]="true"></app-loading-spinner>
              <span *ngIf="!loading">אימות</span>
            </igds-button>
          </form>

          <p *ngIf="error" class="error-text" role="alert">{{ error }}</p>
        </div>
      </igds-card>
    </div>
  `,
  styles: [`
    :host { display: block; direction: inherit; }
    .login-container {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 100vh;
      background-color: var(--igds-bg-neutral);
    }
    .login-card { width: 400px; }
    .login-title {
      font-family: var(--igds-font-family);
      font-size: var(--igds-font-size-xl);
      color: var(--igds-text-primary);
      margin: 0;
    }
    .mfa-text {
      font-family: var(--igds-font-family);
      color: var(--igds-text-secondary);
    }
    .full-width { width: 100%; }
    .error-text {
      color: var(--igds-text-error);
      text-align: center;
      margin-block-start: var(--igds-space-8);
      font-family: var(--igds-font-family);
    }
  `],
})
export class LoginComponent implements OnInit, OnDestroy {
  loginForm!: FormGroup;
  mfaForm!: FormGroup;
  loading = false;
  error: string | null = null;
  mfaRequired = false;
  mfaUserId: string | null = null;
  private destroy$ = new Subject<void>();

  constructor(private fb: FormBuilder, private store: Store<AppState>) {}

  ngOnInit(): void {
    this.loginForm = this.fb.group({
      username: ['', Validators.required],
      password: ['', Validators.required],
    });
    this.mfaForm = this.fb.group({
      code: ['', [Validators.required, Validators.minLength(6)]],
    });

    this.store.select(selectAuthLoading).pipe(takeUntil(this.destroy$)).subscribe((l) => (this.loading = l));
    this.store.select(selectAuthError).pipe(takeUntil(this.destroy$)).subscribe((e) => (this.error = e));
    this.store.select(selectMfaRequired).pipe(takeUntil(this.destroy$)).subscribe((m) => (this.mfaRequired = m));
    this.store.select(selectMfaUserId).pipe(takeUntil(this.destroy$)).subscribe((id) => (this.mfaUserId = id));
  }

  onLogin(): void {
    if (this.loginForm.valid) {
      const { username, password } = this.loginForm.value;
      this.store.dispatch(AuthActions.login({ username, password }));
    }
  }

  onVerifyMfa(): void {
    if (this.mfaForm.valid && this.mfaUserId) {
      this.store.dispatch(AuthActions.verifyMfa({ userId: this.mfaUserId, code: this.mfaForm.value.code }));
    }
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
