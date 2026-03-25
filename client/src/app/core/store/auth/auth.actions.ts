import { createActionGroup, emptyProps, props } from '@ngrx/store';
import { AuthToken, UserProfile } from '../../services/auth.service';

export const AuthActions = createActionGroup({
  source: 'Auth',
  events: {
    'Login': props<{ username: string; password: string }>(),
    'Login Success': props<{ token: AuthToken; requiresMfa: boolean; userId?: string }>(),
    'Login Failure': props<{ error: string }>(),
    'Verify Mfa': props<{ userId: string; code: string }>(),
    'Mfa Success': props<{ token: AuthToken }>(),
    'Mfa Failure': props<{ error: string }>(),
    'Load Profile': emptyProps(),
    'Load Profile Success': props<{ user: UserProfile }>(),
    'Load Profile Failure': props<{ error: string }>(),
    'Logout': emptyProps(),
  },
});
