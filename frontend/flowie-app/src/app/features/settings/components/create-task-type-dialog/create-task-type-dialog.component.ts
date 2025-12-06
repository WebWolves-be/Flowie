import { Component, inject, signal } from "@angular/core";
import { DialogRef } from "@angular/cdk/dialog";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { catchError, EMPTY } from "rxjs";
import { TaskTypeFacade } from "../../facade/task-type.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";

@Component({
  selector: "app-create-task-type-dialog",
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: "./create-task-type-dialog.component.html",
  styleUrl: "./create-task-type-dialog.component.scss"
})
export class CreateTaskTypeDialogComponent {
  #ref = inject(DialogRef);
  #facade = inject(TaskTypeFacade);
  #notificationService = inject(NotificationService);

  form = new FormGroup({
    name: new FormControl("", { nonNullable: true, validators: [Validators.required] })
  });

  errorMessage = signal<string | null>(null);

  onCancel(): void {
    this.#ref.close();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      return;
    }

    this.errorMessage.set(null);

    this.#facade.createTaskType({ name: this.form.value.name! })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.#facade.getTaskTypes();
        this.#notificationService.showSuccess("Taak type succesvol aangemaakt");
        this.#ref.close();
      });
  }
}
