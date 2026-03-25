import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, BehaviorSubject, of, throwError } from 'rxjs';
import { tap, catchError, map } from 'rxjs/operators';
import { environment } from '@env/environment';

export interface LoginRequest {
  username: string;
  password: string;
}

export interface MfaVerifyRequest {
  userId: string;
  code: string;
}

export interface AuthToken {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}

export interface UserProfile {
  id: number;
  username: string;
  displayName: string;
  orgUnitId: number;
  orgUnitName: string;
  roles: string[];
  permissions: string[];
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly TOKEN_KEY = 'auth_token';
  private readonly USER_KEY = 'auth_user';
  private readonly baseUrl = `${environment.apiBaseUrl}/auth`;

  private currentUser$ = new BehaviorSubject<UserProfile | null>(this.getStoredUser());

  constructor(private http: HttpClient) {}

  login(request: LoginRequest): Observable<{ requiresMfa: boolean; userId?: string; token?: AuthToken }> {
    return this.http.post<{ requiresMfa: boolean; userId?: string; token?: AuthToken }>(
      `${this.baseUrl}/login`,
      request
    ).pipe(
      tap((response) => {
        if (!response.requiresMfa && response.token) {
          this.storeToken(response.token);
        }
      })
    );
  }

  verifyMfa(request: MfaVerifyRequest): Observable<AuthToken> {
    return this.http.post<AuthToken>(`${this.baseUrl}/mfa/verify`, request).pipe(
      tap((token) => this.storeToken(token))
    );
  }

  logout(): void {
    localStorage.removeItem(this.TOKEN_KEY);
    localStorage.removeItem(this.USER_KEY);
    this.currentUser$.next(null);
  }

  loadUserProfile(): Observable<UserProfile> {
    return this.http.get<UserProfile>(`${this.baseUrl}/profile`).pipe(
      tap((user) => {
        localStorage.setItem(this.USER_KEY, JSON.stringify(user));
        this.currentUser$.next(user);
      })
    );
  }

  getToken(): string | null {
    const tokenJson = localStorage.getItem(this.TOKEN_KEY);
    if (!tokenJson) return null;
    const token: AuthToken = JSON.parse(tokenJson);
    if (new Date(token.expiresAt) <= new Date()) {
      this.logout();
      return null;
    }
    return token.accessToken;
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  getCurrentUser(): Observable<UserProfile | null> {
    return this.currentUser$.asObservable();
  }

  hasPermission(permission: string): boolean {
    const user = this.currentUser$.value;
    return user?.permissions?.includes(permission) ?? false;
  }

  private storeToken(token: AuthToken): void {
    localStorage.setItem(this.TOKEN_KEY, JSON.stringify(token));
  }

  private getStoredUser(): UserProfile | null {
    const userJson = localStorage.getItem(this.USER_KEY);
    return userJson ? JSON.parse(userJson) : null;
  }
}
