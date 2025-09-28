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

    currentUser = this.#currentUser.asReadonly();
    isAuthenticated = this.#isAuthenticated.asReadonly();

    login(request: LoginRequest): Observable<boolean> {
        return this.http.post<any>(`${this.apiUrl}/login`, request, {
            withCredentials: true,
            observe: 'response'
        }).pipe(
            tap(response => {
                if (response.ok) {
                    const user: User = {
                        email: request.email,
                        isAuthenticated: true
                    };
                    this.#currentUser.set(user);
                    this.#isAuthenticated.set(true);
                }
            }),
            map(() => true),
            catchError(() => of(false))
        );
    }

    checkSession(): Observable<boolean> {
        return this.http.get<any>(`${this.apiUrl}/manage/info`, {withCredentials: true, observe: 'response' as const})
            .pipe(
                map(response => {
                    if (response.ok) {
                        const email = (response.body && (response.body.email || response.body.userName || response.body.user?.email)) ?? this.#currentUser()?.email ?? '';
                        this.#currentUser.set({email, isAuthenticated: true});
                        this.#isAuthenticated.set(true);
                        return true;
                    }
                    return false;
                }),
                catchError(() => {
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