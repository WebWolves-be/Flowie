import { inject, Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { Router } from "@angular/router";
import { catchError, map, Observable, tap, throwError } from "rxjs";
import { environment } from "../../../environments/environment";
import { AuthUser, LoginRequest, RegisterRequest, LogoutRequest, RefreshTokenRequest, TokenResponse } from "../models/auth.model";

@Injectable({
  providedIn: "root"
})
export class AuthFacade {
  #http = inject(HttpClient);
  #router = inject(Router);
  #apiUrl = `${environment.apiUrl}/auth`;

  #isAuthenticated = signal(false);
  #currentUser = signal<AuthUser | null>(null);
  #refreshTimerId: ReturnType<typeof setTimeout> | null = null;

  isAuthenticated = this.#isAuthenticated.asReadonly();
  currentUser = this.#currentUser.asReadonly();

  constructor() {
    this.#restoreAuthState();
  }

  login(request: LoginRequest): Observable<void> {
    return this.#http.post<TokenResponse>(`${this.#apiUrl}/login`, request).pipe(
      tap((response) => {
        this.#storeTokens(response);
        this.#updateAuthState(response);
      }),
      map(() => void 0),
      catchError((error) => {
        return throwError(() => error);
      })
    );
  }

  register(request: RegisterRequest): Observable<void> {
    return this.#http.post<TokenResponse>(`${this.#apiUrl}/register`, request).pipe(
      tap((response) => {
        this.#storeTokens(response);
        this.#updateAuthState(response);
      }),
      map(() => void 0),
      catchError((error) => {
        return throwError(() => error);
      })
    );
  }

  logout(): Observable<void> {
    const refreshToken = localStorage.getItem("refresh_token");

    if (refreshToken) {
      const request: LogoutRequest = { refreshToken };
      return this.#http.post<void>(`${this.#apiUrl}/logout`, request).pipe(
        tap(() => {
          this.#clearAuthState();
        }),
        catchError((error) => {
          this.#clearAuthState();
          return throwError(() => error);
        })
      );
    }

    this.#clearAuthState();
    return new Observable((observer) => {
      observer.next();
      observer.complete();
    });
  }

  refreshToken(): Observable<void> {
    const refreshToken = localStorage.getItem("refresh_token");

    if (!refreshToken) {
      this.#clearAuthState();
      return throwError(() => new Error("No refresh token available"));
    }

    const request: RefreshTokenRequest = { refreshToken };

    return this.#http.post<TokenResponse>(`${this.#apiUrl}/refresh`, request).pipe(
      tap((response) => {
        this.#storeTokens(response);
        this.#scheduleTokenRefresh(new Date(response.expiresAt));
      }),
      map(() => void 0),
      catchError((error) => {
        this.#clearAuthState();
        return throwError(() => error);
      })
    );
  }

  getAccessToken(): string | null {
    return localStorage.getItem("access_token");
  }

  #storeTokens(response: TokenResponse): void {
    localStorage.setItem("access_token", response.accessToken);
    localStorage.setItem("refresh_token", response.refreshToken);
    localStorage.setItem("expires_at", response.expiresAt);
  }

  #updateAuthState(response: TokenResponse): void {
    const user = this.#parseJwt(response.accessToken);
    if (user) {
      this.#currentUser.set(user);
      this.#isAuthenticated.set(true);
      this.#scheduleTokenRefresh(new Date(response.expiresAt));
    }
  }

  #restoreAuthState(): void {
    const accessToken = localStorage.getItem("access_token");
    const expiresAt = localStorage.getItem("expires_at");

    if (!accessToken || !expiresAt) {
      return;
    }

    const expiryDate = new Date(expiresAt);
    const now = new Date();

    if (expiryDate <= now) {
      this.refreshToken().subscribe();
      return;
    }

    const user = this.#parseJwt(accessToken);
    if (user) {
      this.#currentUser.set(user);
      this.#isAuthenticated.set(true);
      this.#scheduleTokenRefresh(expiryDate);
    }
  }

  #clearAuthState(): void {
    if (this.#refreshTimerId) {
      clearTimeout(this.#refreshTimerId);
      this.#refreshTimerId = null;
    }

    localStorage.removeItem("access_token");
    localStorage.removeItem("refresh_token");
    localStorage.removeItem("expires_at");

    this.#currentUser.set(null);
    this.#isAuthenticated.set(false);

    this.#router.navigate(["/login"]);
  }

  #scheduleTokenRefresh(expiresAt: Date): void {
    if (this.#refreshTimerId) {
      clearTimeout(this.#refreshTimerId);
    }

    const now = new Date();
    const refreshBuffer = 5 * 60 * 1000;
    const timeUntilRefresh = expiresAt.getTime() - now.getTime() - refreshBuffer;

    if (timeUntilRefresh <= 0) {
      this.refreshToken().subscribe();
      return;
    }

    this.#refreshTimerId = setTimeout(() => {
      this.refreshToken().subscribe();
    }, timeUntilRefresh);
  }

  #parseJwt(token: string): AuthUser | null {
    try {
      const base64Url = token.split(".")[1];
      const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
      const jsonPayload = decodeURIComponent(
        atob(base64)
          .split("")
          .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
          .join("")
      );

      const payload = JSON.parse(jsonPayload);

      return {
        userId: payload.user_id || payload.sub,
        employeeId: parseInt(payload.employee_id),
        email: payload.email,
        name: payload.name
      };
    } catch {
      return null;
    }
  }
}
