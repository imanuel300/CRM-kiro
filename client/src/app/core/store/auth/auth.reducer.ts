import { createReducer, on } from '@ngrx/store';
import { AuthActions } from './auth.actions';
import { UserProfile } from '../../services/auth.service';

export interface AuthState {
  isAuthenticated: boolean;
  user: UserProfile | null;
  loading: boolean;
  error: string | null;
  mfaRequired: boolean;
  mfaUserId: string | null;
}

export const initialAuthState: AuthState = {
  isAuthenticated: !!localStorage.getItem('auth_token'),
  user: (() => {
    const u = localStorage.getItem('auth_user');
    return u ? JSON.parse(u) : null;
  })(),
  loading: false,
  error: null,
  mfaRequired: false,
  mfaUserId: null,
};

export const authReducer = createReducer(
  initialAuthState,

  on(AuthActions.login, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(AuthActions.loginSuccess, (state, { requiresMfa, userId }) => ({
    ...state,
    loading: false,
    isAuthenticated: !requiresMfa,
    mfaRequired: requiresMfa,
    mfaUserId: userId ?? null,
  })),

  on(AuthActions.loginFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(AuthActions.verifyMfa, (state) => ({
    ...state,
    loading: true,
    error: null,
  })),

  on(AuthActions.mfaSuccess, (state) => ({
    ...state,
    loading: false,
    isAuthenticated: true,
    mfaRequired: false,
    mfaUserId: null,
  })),

  on(AuthActions.mfaFailure, (state, { error }) => ({
    ...state,
    loading: false,
    error,
  })),

  on(AuthActions.loadProfileSuccess, (state, { user }) => ({
    ...state,
    user,
  })),

  on(AuthActions.logout, () => ({
    ...initialAuthState,
    isAuthenticated: false,
    user: null,
  }))
);
