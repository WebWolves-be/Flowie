import { Component, inject, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { HttpErrorResponse } from "@angular/common/http";
import { catchError, EMPTY } from "rxjs";
import { TaskTypeFacade } from "../../facade/task-type.facade";
import { DeleteTaskTypeDialogData } from "../../models/delete-task-type-dialog-data.model";
import { NotificationService } from "../../../../core/services/notification.service";

interface ValidationError {
  errorMessage: string;
}

@Component({
  selector: "app-delete-task-type-dialog",
  standalone: true,
  imports: [],
  templateUrl: "./delete-task-type-dialog.component.html",
  styleUrl: "./delete-task-type-dialog.component.scss"
})
export class DeleteTaskTypeDialogComponent {
  #ref = inject(DialogRef);
  #data = inject<DeleteTaskTypeDialogData>(DIALOG_DATA);
  #facade = inject(TaskTypeFacade);
  #notificationService = inject(NotificationService);

  readonly taskType = this.#data.taskType;
  readonly errorMessage = signal<string | null>(null);

  onCancel(): void {
    this.#ref.close();
  }

  onConfirm(): void {
    this.errorMessage.set(null);

    this.#facade.deleteTaskType(this.taskType.taskTypeId)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(this.#extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.#facade.getTaskTypes();
        this.#notificationService.showSuccess("Taak type succesvol verwijderd");
        this.#ref.close();
      });
  }

  #extractErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400 && error.error?.errors) {
      const errors = error.error.errors as ValidationError[];
      if (Array.isArray(errors) && errors.length > 0) {
        return errors.map((e) => e.errorMessage).join(" ");
      }
    }
    if (error.error?.title) {
      return error.error.title;
    }
    return "Er is een fout opgetreden. Probeer het opnieuw.";
  }
}
