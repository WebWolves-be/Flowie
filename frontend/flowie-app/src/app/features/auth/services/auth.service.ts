import {inject, Injectable, signal} from "@angular/core";
import {HttpClient} from '@angular/common/http';
import {catchError, map, Observable, of, tap, switchMap} from 'rxjs';
import {LoginRequest} from "../models/login-request.model";
import {User} from "../models/user.model";
import {TokenService, TokenResponse} from './token.service';

@Injectable({providedIn: 'root'})
export class AuthService {
    private readonly http = inject(HttpClient);
    private readonly tokenService = inject(TokenService);

    private readonly apiUrl = 'https://localhost:7067';

    readonly #currentUser = signal<User | null>(null);
    readonly #isAuthenticated = signal<boolean>(false);

    currentUser = this.#currentUser.asReadonly();
    isAuthenticated = this.#isAuthenticated.asReadonly();
    
    constructor() {
        // Initialize auth state on service creation
        this.initializeAuthState();
        
        // Listen for token changes
        this.tokenService.getTokenObservable().subscribe(token => {
            if (token) {
                this.updateUserFromToken(token);
            } else {
                this.clearAuthState();
            }
        });
    }

    login(request: LoginRequest): Observable<boolean> {
        console.log('🔐 AuthService: Attempting login for:', request.email);
        console.log('🔐 AuthService: Using URL:', `${this.apiUrl}/auth/login`);
        
        return this.http.post<TokenResponse>(`${this.apiUrl}/auth/login`, request).pipe(
            tap(tokenResponse => {
                console.log('🔐 AuthService: Login successful, storing tokens');
                this.tokenService.setTokens(tokenResponse);
                this.updateUserFromToken(tokenResponse.accessToken);
            }),
            map(() => true),
            catchError(error => {
                console.error('❌ AuthService: Login error:', error);
                this.clearAuthState();
                return of(false);
            })
        );
    }

    /**
     * Check current authentication status
     */
    checkSession(): Observable<boolean> {
        console.log('� AuthService: Checking session...');
        
        // Check if we have valid tokens
        if (!this.tokenService.hasValidTokens()) {
            console.log('❌ AuthService: No valid tokens found');
            this.clearAuthState();
            return of(false);
        }
        
        // Get user info from backend to validate session
        return this.http.get<any>(`${this.apiUrl}/auth/me`).pipe(
            map(userInfo => {
                console.log('🔍 AuthService: User info received:', userInfo);
                this.#currentUser.set({
                    email: userInfo.email || userInfo.userName || '',
                    isAuthenticated: true
                });
                this.#isAuthenticated.set(true);
                console.log('✅ AuthService: Session valid, user authenticated');
                return true;
            }),
            catchError(error => {
                console.error('❌ AuthService: Session check error:', error);
                this.clearAuthState();
                return of(false);
            })
        );
    }

    logout(): Observable<boolean> {
        const refreshToken = this.tokenService.getRefreshToken();
        
        if (!refreshToken) {
            // No refresh token, just clear local state
            this.clearAuthState();
            return of(true);
        }
        
        console.log('🚪 AuthService: Logging out...');
        
        return this.http.post(`${this.apiUrl}/auth/logout`, {
            refreshToken: refreshToken
        }).pipe(
            tap(() => {
                console.log('✅ AuthService: Server logout successful');
            }),
            map(() => {
                this.clearAuthState();
                return true;
            }),
            catchError(error => {
                console.error('❌ AuthService: Logout error:', error);
                // Clear local state even if server logout fails
                this.clearAuthState();
                return of(true);
            })
        );
    }
    
    /**
     * Initialize authentication state on service creation
     */
    private initializeAuthState(): void {
        if (this.tokenService.hasValidTokens()) {
            const accessToken = this.tokenService.getAccessToken();
            if (accessToken) {
                this.updateUserFromToken(accessToken);
            }
        }
    }
    
    /**
     * Update user information from JWT token
     */
    private updateUserFromToken(token: string): void {
        const payload = this.tokenService.parseTokenPayload(token);
        if (payload) {
            this.#currentUser.set({
                email: payload.email || '',
                isAuthenticated: true
            });
            this.#isAuthenticated.set(true);
            console.log('✅ AuthService: User updated from token');
        }
    }
    
    /**
     * Clear authentication state
     */
    private clearAuthState(): void {
        this.tokenService.clearTokens();
        this.#currentUser.set(null);
        this.#isAuthenticated.set(false);
        console.log('🧹 AuthService: Auth state cleared');
    }
    
    /**
     * Get current user ID from token
     */
    getCurrentUserId(): string | null {
        const accessToken = this.tokenService.getAccessToken();
        if (accessToken) {
            const payload = this.tokenService.parseTokenPayload(accessToken);
            return payload?.user_id || payload?.sub || null;
        }
        return null;
    }
    
    /**
     * Check if current user has valid authentication
     */
    isCurrentlyAuthenticated(): boolean {
        return this.tokenService.hasValidTokens() && this.#isAuthenticated();
    }
}