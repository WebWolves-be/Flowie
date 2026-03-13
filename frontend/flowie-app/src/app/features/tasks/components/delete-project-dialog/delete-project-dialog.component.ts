import { Component, inject, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { HttpErrorResponse } from "@angular/common/http";
import { catchError, EMPTY } from "rxjs";
import { TaskFacade } from "../../task.facade";
import { DeleteProjectDialogData } from "../../models/delete-project-dialog-data.model";
import { NotificationService } from "../../../../core/services/notification.service";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";

@Component({
  selector: "app-delete-project-dialog",
  standalone: true,
  imports: [],
  templateUrl: "./delete-project-dialog.component.html",
  styleUrl: "./delete-project-dialog.component.scss"
})
export class DeleteProjectDialogComponent {
  #dialogRef = inject(DialogRef);
  #dialogData = inject<DeleteProjectDialogData>(DIALOG_DATA);
  #facade = inject(TaskFacade);
  #notificationService = inject(NotificationService);

  readonly project = this.#dialogData.project;
  readonly errorMessage = signal<string | null>(null);

  onCancel(): void {
    this.#dialogRef.close();
  }

  onConfirm(): void {
    this.errorMessage.set(null);

    this.#facade.deleteProject(this.project.projectId)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.#facade.getProjects();
        this.#notificationService.showSuccess("Project succesvol verwijderd");
        this.#dialogRef.close(true);
      });
  }
}
