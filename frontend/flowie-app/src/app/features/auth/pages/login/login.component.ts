import {Component, inject, OnInit, signal} from '@angular/core';
import {FormBuilder, ReactiveFormsModule, Validators} from '@angular/forms';
import {Router, RouterLink} from '@angular/router';
import {AuthService, LoginRequest} from '../../services/auth.service';
import {CommonModule} from '@angular/common';
import {catchError, EMPTY, finalize} from "rxjs";

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
            return;
        }

        this.#authService.checkSession().subscribe(isAuthenticated => {
            if (isAuthenticated) {
                void this.#router.navigate(['/dashboard']);
            }
        });
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
                    finalize(() => {
                        this.isLoading.set(false);
                    }),
                    catchError(() => {
                        this.errorMessage.set('Er is iets verkeerd gegaan bij het inloggen. Controleer je gegevens en probeer het opnieuw.');
                        return EMPTY;
                    }))
                .subscribe(() => {
                    void this.#router.navigate(['/dashboard']);
                });
        }
    }
}
