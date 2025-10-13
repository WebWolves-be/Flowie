import { Injectable, inject } from '@angular/core';
import { BehaviorSubject, Observable, timer, switchMap, catchError, of, tap, filter } from 'rxjs';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  tokenType?: string;
}

export interface JwtPayload {
  sub: string;
  email: string;
  jti: string;
  iss: string;
  aud: string;
  iat: number;
  exp: number;
  nbf: number;
  user_id: string;
  token_type: string;
  scope: string;
}

@Injectable({
  providedIn: 'root'
})
export class TokenService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = 'https://localhost:7067';
  
  private readonly ACCESS_TOKEN_KEY = 'flowie_access_token';
  private readonly REFRESH_TOKEN_KEY = 'flowie_refresh_token';
  private readonly TOKEN_EXPIRY_KEY = 'flowie_token_expiry';
  
  private refreshTimer?: any;
  private readonly tokenRefreshSubject = new BehaviorSubject<string | null>(null);
  
  constructor() {
    this.initializeTokenRefresh();
  }
  
  /**
   * Store tokens securely
   */
  setTokens(tokenResponse: TokenResponse): void {
    try {
      // Store tokens in sessionStorage (more secure than localStorage for sensitive data)
      sessionStorage.setItem(this.ACCESS_TOKEN_KEY, tokenResponse.accessToken);
      sessionStorage.setItem(this.REFRESH_TOKEN_KEY, tokenResponse.refreshToken);
      sessionStorage.setItem(this.TOKEN_EXPIRY_KEY, tokenResponse.expiresAt);
      
      this.tokenRefreshSubject.next(tokenResponse.accessToken);
      this.scheduleTokenRefresh(tokenResponse.expiresAt);
      
      console.log('✅ TokenService: Tokens stored successfully');
    } catch (error) {
      console.error('❌ TokenService: Failed to store tokens:', error);
    }
  }
  
  /**
   * Get access token
   */
  getAccessToken(): string | null {
    try {
      const token = sessionStorage.getItem(this.ACCESS_TOKEN_KEY);
      
      if (token && this.isTokenValid(token)) {
        return token;
      }
      
      // Token is invalid or expired
      this.clearTokens();
      return null;
    } catch (error) {
      console.error('❌ TokenService: Failed to get access token:', error);
      return null;
    }
  }
  
  /**
   * Get refresh token
   */
  getRefreshToken(): string | null {
    try {
      return sessionStorage.getItem(this.REFRESH_TOKEN_KEY);
    } catch (error) {
      console.error('❌ TokenService: Failed to get refresh token:', error);
      return null;
    }
  }
  
  /**
   * Check if we have valid tokens
   */
  hasValidTokens(): boolean {
    const accessToken = this.getAccessToken();
    const refreshToken = this.getRefreshToken();
    
    return !!(accessToken && refreshToken);
  }
  
  /**
   * Clear all stored tokens
   */
  clearTokens(): void {
    try {
      sessionStorage.removeItem(this.ACCESS_TOKEN_KEY);
      sessionStorage.removeItem(this.REFRESH_TOKEN_KEY);
      sessionStorage.removeItem(this.TOKEN_EXPIRY_KEY);
      
      this.tokenRefreshSubject.next(null);
      this.clearRefreshTimer();
      
      console.log('✅ TokenService: Tokens cleared');
    } catch (error) {
      console.error('❌ TokenService: Failed to clear tokens:', error);
    }
  }
  
  /**
   * Refresh access token using refresh token
   */
  refreshTokens(): Observable<TokenResponse | null> {
    const refreshToken = this.getRefreshToken();
    
    if (!refreshToken) {
      console.warn('⚠️ TokenService: No refresh token available');
      return of(null);
    }
    
    console.log('🔄 TokenService: Refreshing tokens...');
    
    return this.http.post<TokenResponse>(`${this.apiUrl}/auth/refresh`, {
      refreshToken: refreshToken
    }).pipe(
      tap(response => {
        if (response) {
          this.setTokens(response);
          console.log('✅ TokenService: Tokens refreshed successfully');
        }
      }),
      catchError((error: HttpErrorResponse) => {
        console.error('❌ TokenService: Token refresh failed:', error);
        
        // If refresh fails, clear all tokens
        if (error.status === 401) {
          console.log('🚫 TokenService: Refresh token invalid, clearing all tokens');
          this.clearTokens();
        }
        
        return of(null);
      })
    );
  }
  
  /**
   * Get token expiration time
   */
  getTokenExpiration(): Date | null {
    try {
      const expiryString = sessionStorage.getItem(this.TOKEN_EXPIRY_KEY);
      if (expiryString) {
        return new Date(expiryString);
      }
      return null;
    } catch (error) {
      console.error('❌ TokenService: Failed to get token expiration:', error);
      return null;
    }
  }
  
  /**
   * Parse JWT payload without verification (client-side only for UI purposes)
   */
  parseTokenPayload(token: string): JwtPayload | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) {
        return null;
      }
      
      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      
      return JSON.parse(decoded) as JwtPayload;
    } catch (error) {
      console.error('❌ TokenService: Failed to parse token payload:', error);
      return null;
    }
  }
  
  /**
   * Check if token is valid (not expired)
   */
  private isTokenValid(token: string): boolean {
    const payload = this.parseTokenPayload(token);
    if (!payload) {
      return false;
    }
    
    const now = Math.floor(Date.now() / 1000);
    return payload.exp > now;
  }
  
  /**
   * Initialize automatic token refresh
   */
  private initializeTokenRefresh(): void {
    const accessToken = this.getAccessToken();
    if (accessToken) {
      const expiry = this.getTokenExpiration();
      if (expiry) {
        this.scheduleTokenRefresh(expiry.toISOString());
      }
    }
  }
  
  /**
   * Schedule automatic token refresh
   */
  private scheduleTokenRefresh(expiresAt: string): void {
    this.clearRefreshTimer();
    
    const expiryTime = new Date(expiresAt).getTime();
    const currentTime = Date.now();
    const timeUntilExpiry = expiryTime - currentTime;
    
    // Refresh token 5 minutes before expiry, or immediately if less than 5 minutes
    const refreshBuffer = 5 * 60 * 1000; // 5 minutes in milliseconds
    const refreshTime = Math.max(timeUntilExpiry - refreshBuffer, 1000); // At least 1 second
    
    console.log(`🕐 TokenService: Scheduling token refresh in ${Math.round(refreshTime / 1000)} seconds`);
    
    this.refreshTimer = timer(refreshTime).pipe(
      switchMap(() => this.refreshTokens()),
      filter(response => !!response)
    ).subscribe({
      next: () => {
        console.log('✅ TokenService: Automatic token refresh completed');
      },
      error: (error) => {
        console.error('❌ TokenService: Automatic token refresh failed:', error);
      }
    });
  }
  
  /**
   * Clear refresh timer
   */
  private clearRefreshTimer(): void {
    if (this.refreshTimer) {
      this.refreshTimer.unsubscribe();
      this.refreshTimer = undefined;
    }
  }
  
  /**
   * Get observable for token changes
   */
  getTokenObservable(): Observable<string | null> {
    return this.tokenRefreshSubject.asObservable();
  }
}