import {Component, inject, OnInit, signal} from '@angular/core';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {Router, RouterLink} from '@angular/router';
import {AuthService} from '../../services/auth.service';
import {CommonModule} from '@angular/common';
import {catchError, EMPTY, finalize, delay, switchMap} from "rxjs";
import {LoginRequest} from "../../models/login-request.model";

@Component({
    selector: 'app-login',
    imports: [ReactiveFormsModule, CommonModule, RouterLink],
    templateUrl: './login.component.html',
    styleUrl: './login.component.scss'
})
export class LoginComponent implements OnInit {
    #formBuilder = inject(FormBuilder);
    #authService = inject(AuthService);
    #router = inject(Router);

    isLoading = signal(false);
    errorMessage = signal<string | null>(null);

    loginForm = this.#formBuilder.group({
        email: [null, [Validators.required, Validators.email]],
        password: [null, [Validators.required, Validators.minLength(10)]]
    });

    get email() {
        return this.loginForm.get('email');
    }

    get password() {
        return this.loginForm.get('password');
    }

    ngOnInit() {
        if (this.#authService.isAuthenticated()) {
            void this.#router.navigate(['/dashboard']);
        }
    }

    onSubmit() {
        if (this.loginForm.valid) {
            this.isLoading.set(true);
            this.errorMessage.set(null);

            const loginRequest: LoginRequest = {
                email: this.loginForm.value.email!,
                password: this.loginForm.value.password!
            };

            this.#authService.login(loginRequest)
                .pipe(
                    switchMap((success) => {
                        if (success) {
                            console.log('Login component: Login successful, waiting 500ms before session check...');
                            // Wait a bit for cookies to be fully set, then check session
                            return this.#authService.checkSession().pipe(delay(500));
                        } else {
                            console.error('Login component: Login failed');
                            this.errorMessage.set('Login failed. Please check your credentials.');
                            return EMPTY;
                        }
                    }),
                    finalize(() => {
                        this.isLoading.set(false);
                    }),
                    catchError((error) => {
                        console.error('Login component: Login or session check failed', error);
                        this.errorMessage.set('Er is iets verkeerd gegaan bij het inloggen. Controleer je gegevens en probeer het opnieuw.');
                        return EMPTY;
                    })
                )
                .subscribe((sessionValid) => {
                    if (sessionValid) {
                        console.log('Login component: Session validated successfully, navigating to dashboard');
                        void this.#router.navigate(['/dashboard']);
                    } else {
                        console.error('Login component: Session validation failed after login');
                        this.errorMessage.set('Login succeeded but session validation failed. Please try again.');
                    }
                });
        }
    }
}
