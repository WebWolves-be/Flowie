import { Component, inject, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { AuthFacade } from "../../../../core/facades/auth.facade";
import { HttpErrorResponse } from "@angular/common/http";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";
import { catchError, EMPTY } from "rxjs";

@Component({
  selector: "app-login-page",
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: "./login-page.component.html",
  styleUrl: "./login-page.component.scss"
})
export class LoginPageComponent {
  #authFacade = inject(AuthFacade);
  #router = inject(Router);

  errorMessage = signal<string | null>(null);
  isLoading = signal(false);

  loginForm = new FormGroup({
    email: new FormControl("", { nonNullable: true, validators: [Validators.required] }),
    password: new FormControl("", { nonNullable: true, validators: [Validators.required] })
  });

  get email() {
    return this.loginForm.get("email");
  }

  get password() {
    return this.loginForm.get("password");
  }

  onSubmit(): void {
    if (this.loginForm.invalid) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const emailTrimmed = this.loginForm.value.email!.trim();
    const passwordTrimmed = this.loginForm.value.password!.trim();

    if (!emailTrimmed) {
      this.loginForm.controls.email.setErrors({ required: true });
      return;
    }

    if (!passwordTrimmed) {
      this.loginForm.controls.password.setErrors({ required: true });
      return;
    }

    this.errorMessage.set(null);
    this.isLoading.set(true);

    const request = {
      email: emailTrimmed,
      password: passwordTrimmed
    };

    this.#authFacade.login(request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.isLoading.set(false);
          if (error.status === 401) {
            this.errorMessage.set("Ongeldige e-mail of wachtwoord");
          } else {
            this.errorMessage.set(extractErrorMessage(error));
          }
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.isLoading.set(false);
        this.#router.navigate(["/dashboard"]);
      });
  }
}
