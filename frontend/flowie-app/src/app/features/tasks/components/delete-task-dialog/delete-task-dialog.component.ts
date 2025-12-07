import { Component, inject, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { HttpErrorResponse } from "@angular/common/http";
import { catchError, EMPTY } from "rxjs";
import { TaskFacade } from "../../task.facade";
import { DeleteTaskDialogData } from "../../models/delete-task-dialog-data.model";
import { NotificationService } from "../../../../core/services/notification.service";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";

@Component({
  selector: "app-delete-task-dialog",
  standalone: true,
  imports: [],
  templateUrl: "./delete-task-dialog.component.html",
  styleUrl: "./delete-task-dialog.component.scss"
})
export class DeleteTaskDialogComponent {
  #dialogRef = inject(DialogRef);
  #dialogData = inject<DeleteTaskDialogData>(DIALOG_DATA);
  #facade = inject(TaskFacade);
  #notificationService = inject(NotificationService);

  readonly task = this.#dialogData.task;
  readonly isSubtask = this.#dialogData.isSubtask ?? false;
  readonly errorMessage = signal<string | null>(null);

  onCancel(): void {
    this.#dialogRef.close();
  }

  onConfirm(): void {
    this.errorMessage.set(null);

    const successMessage = this.isSubtask ? "Subtaak succesvol verwijderd" : "Taak succesvol verwijderd";

    this.#facade.deleteTask(this.task.taskId)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.#notificationService.showSuccess(successMessage);
        this.#dialogRef.close(true);
      });
  }
}