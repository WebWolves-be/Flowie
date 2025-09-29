import {inject, Injectable, signal} from "@angular/core";
import {HttpClient} from '@angular/common/http';
import {catchError, map, Observable, of, tap} from 'rxjs';
import {LoginRequest} from "../models/login-request.model";
import {User} from "../models/user.model";

@Injectable({providedIn: 'root'})
export class AuthService {
    private readonly http = inject(HttpClient);

    private readonly apiUrl = 'https://localhost:7067';

    readonly #currentUser = signal<User | null>(null);
    readonly #isAuthenticated = signal<boolean>(false);

    // Temporary storage for development debugging
    private sessionToken: string | null = null;

    currentUser = this.#currentUser.asReadonly();
    isAuthenticated = this.#isAuthenticated.asReadonly();

    login(request: LoginRequest): Observable<boolean> {
        console.log('🔐 AuthService: Attempting login for:', request.email);
        console.log('🔐 AuthService: Using URL:', `${this.apiUrl}/login`);
        console.log('🔐 AuthService: withCredentials: true');
        
        return this.http.post<any>(`${this.apiUrl}/login`, request, {
            withCredentials: true,
            observe: 'response'
        }).pipe(
            tap(response => {
                console.log('🔐 AuthService: Login response status:', response.status);
                console.log('🔐 AuthService: Login response headers:', response.headers);
                console.log('🔐 AuthService: Login response body:', response.body);
            }),
            // After successful login, immediately check session to validate authentication
            map(() => true),
            catchError(error => {
                console.error('❌ AuthService: Login error:', error);
                console.log('❌ AuthService: Error status:', error.status);
                console.log('❌ AuthService: Error message:', error.message);
                this.#currentUser.set(null);
                this.#isAuthenticated.set(false);
                return of(false);
            })
        );
    }

    /**
     * Login and immediately validate session
     */
    loginAndValidate(request: LoginRequest): Observable<boolean> {
        return this.login(request).pipe(
            map(loginSuccess => {
                if (loginSuccess) {
                    console.log('🔐 AuthService: Login successful, validating session...');
                    // Don't set auth state here - let checkSession do it
                    return true;
                } else {
                    console.log('❌ AuthService: Login failed');
                    return false;
                }
            })
        );
    }

    checkSession(): Observable<boolean> {
        console.log('🔍 AuthService: Checking session...');
        console.log('🔍 AuthService: Current browser cookies:', document.cookie);
        console.log('🔍 AuthService: Using URL:', `${this.apiUrl}/manage/info`);
        
        return this.http.get<any>(`${this.apiUrl}/manage/info`, {withCredentials: true, observe: 'response' as const})
            .pipe(
                map(response => {
                    console.log('🔍 AuthService: Session check response status:', response.status);
                    console.log('🔍 AuthService: Session check response headers:', response.headers);
                    console.log('🔍 AuthService: Session check response body:', response.body);
                    
                    if (response.ok) {
                        const email = (response.body && (response.body.email || response.body.userName || response.body.user?.email)) ?? this.#currentUser()?.email ?? '';
                        console.log('🔍 AuthService: Extracted email:', email);
                        this.#currentUser.set({email, isAuthenticated: true});
                        this.#isAuthenticated.set(true);
                        console.log('✅ AuthService: Session valid, user authenticated');
                        return true;
                    }
                    console.log('❌ AuthService: Session check failed, response not OK');
                    return false;
                }),
                catchError(error => {
                    console.error('❌ AuthService: Session check error:', error);
                    console.log('❌ AuthService: Session check error status:', error.status);
                    console.log('❌ AuthService: Session check error message:', error.message);
                    this.#currentUser.set(null);
                    this.#isAuthenticated.set(false);
                    return of(false);
                })
            );
    }

    logout(): Observable<boolean> {
        return this.http.post(`${this.apiUrl}/logout`, {}, {withCredentials: true, observe: 'response' as const})
            .pipe(
                map(() => {
                    this.#currentUser.set(null);
                    this.#isAuthenticated.set(false);
                    return true;
                }),
                catchError(() => {
                    this.#currentUser.set(null);
                    this.#isAuthenticated.set(false);
                    return of(true);
                })
            );
    }
}