import { Component, inject, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { HttpErrorResponse } from "@angular/common/http";
import { catchError, EMPTY } from "rxjs";
import { TaskFacade } from "../../task.facade";
import { DeleteSectionDialogData } from "../../models/delete-section-dialog-data.model";
import { NotificationService } from "../../../../core/services/notification.service";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";

@Component({
  selector: "app-delete-section-dialog",
  standalone: true,
  imports: [],
  templateUrl: "./delete-section-dialog.component.html",
  styleUrl: "./delete-section-dialog.component.scss"
})
export class DeleteSectionDialogComponent {
  #dialogRef = inject(DialogRef);
  #dialogData = inject<DeleteSectionDialogData>(DIALOG_DATA);
  #facade = inject(TaskFacade);
  #notificationService = inject(NotificationService);

  readonly section = this.#dialogData.section;
  readonly errorMessage = signal<string | null>(null);

  onCancel(): void {
    this.#dialogRef.close();
  }

  onConfirm(): void {
    this.errorMessage.set(null);

    this.#facade.deleteSection(this.section.sectionId)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.#facade.getSections(this.#dialogData.projectId);
        this.#facade.getTasks(this.#dialogData.projectId);
        this.#notificationService.showSuccess("Sectie succesvol verwijderd");
        this.#dialogRef.close(true);
      });
  }
}
