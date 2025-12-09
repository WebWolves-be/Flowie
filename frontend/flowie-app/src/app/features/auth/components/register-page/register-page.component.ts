import { Component, inject, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { AuthFacade } from "../../../../core/facades/auth.facade";
import { HttpErrorResponse } from "@angular/common/http";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";
import { catchError, EMPTY } from "rxjs";

@Component({
  selector: "app-register-page",
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: "./register-page.component.html",
  styleUrl: "./register-page.component.scss"
})
export class RegisterPageComponent {
  #authFacade = inject(AuthFacade);
  #router = inject(Router);

  errorMessage = signal<string | null>(null);
  isLoading = signal(false);

  registerForm = new FormGroup({
    firstName: new FormControl("", { nonNullable: true, validators: [Validators.required] }),
    lastName: new FormControl("", { nonNullable: true, validators: [Validators.required] }),
    email: new FormControl("", { nonNullable: true, validators: [Validators.required] }),
    password: new FormControl("", { nonNullable: true, validators: [Validators.required] }),
    registrationCode: new FormControl("", { nonNullable: true, validators: [Validators.required] })
  });

  get firstName() {
    return this.registerForm.get("firstName");
  }

  get lastName() {
    return this.registerForm.get("lastName");
  }

  get email() {
    return this.registerForm.get("email");
  }

  get password() {
    return this.registerForm.get("password");
  }

  get registrationCode() {
    return this.registerForm.get("registrationCode");
  }

  onSubmit(): void {
    if (this.registerForm.invalid) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const firstNameTrimmed = this.registerForm.value.firstName!.trim();
    const lastNameTrimmed = this.registerForm.value.lastName!.trim();
    const emailTrimmed = this.registerForm.value.email!.trim();
    const passwordTrimmed = this.registerForm.value.password!.trim();
    const registrationCodeTrimmed = this.registerForm.value.registrationCode!.trim();

    if (!firstNameTrimmed) {
      this.registerForm.controls.firstName.setErrors({ required: true });
      return;
    }

    if (!lastNameTrimmed) {
      this.registerForm.controls.lastName.setErrors({ required: true });
      return;
    }

    if (!emailTrimmed) {
      this.registerForm.controls.email.setErrors({ required: true });
      return;
    }

    if (!passwordTrimmed) {
      this.registerForm.controls.password.setErrors({ required: true });
      return;
    }

    if (!registrationCodeTrimmed) {
      this.registerForm.controls.registrationCode.setErrors({ required: true });
      return;
    }

    this.errorMessage.set(null);
    this.isLoading.set(true);

    const request = {
      firstName: firstNameTrimmed,
      lastName: lastNameTrimmed,
      email: emailTrimmed,
      password: passwordTrimmed,
      registrationCode: registrationCodeTrimmed
    };

    this.#authFacade.register(request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.isLoading.set(false);
          this.errorMessage.set(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.isLoading.set(false);
        this.#router.navigate(["/dashboard"]);
      });
  }
}
