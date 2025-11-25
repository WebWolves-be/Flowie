import { Component, inject, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { CommonModule } from "@angular/common";
import { HttpErrorResponse } from "@angular/common/http";
import { TaskTypeFacade, TaskType } from "../../facade/task-type.facade";

export interface ConfirmDeleteDialogData {
  taskType: TaskType;
}

export interface ConfirmDeleteDialogResult {
  deleted: boolean;
}

@Component({
  selector: "app-confirm-delete-dialog",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./confirm-delete-dialog.component.html",
  styleUrl: "./confirm-delete-dialog.component.scss"
})
export class ConfirmDeleteDialogComponent {
  private ref = inject<DialogRef<ConfirmDeleteDialogResult>>(DialogRef);
  private data = inject<ConfirmDeleteDialogData>(DIALOG_DATA);
  private facade = inject(TaskTypeFacade);

  readonly taskType = this.data.taskType;
  readonly isDeleting = signal(false);
  readonly errorMessage = signal<string | null>(null);

  onCancel(): void {
    this.ref.close({ deleted: false });
  }

  onConfirm(): void {
    if (this.isDeleting()) {
      return;
    }

    this.errorMessage.set(null);
    this.isDeleting.set(true);

    this.facade.remove(this.taskType.id).subscribe({
      next: () => {
        this.isDeleting.set(false);
        this.ref.close({ deleted: true });
      },
      error: (error: HttpErrorResponse) => {
        this.isDeleting.set(false);
        this.errorMessage.set(this.extractErrorMessage(error));
      }
    });
  }

  private extractErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400 && error.error?.errors) {
      // FluentValidation error format
      const errors = error.error.errors;
      if (Array.isArray(errors) && errors.length > 0) {
        return errors.map((e: any) => e.errorMessage).join(" ");
      }
    }
    if (error.error?.title) {
      return error.error.title;
    }
    return "Er is een fout opgetreden. Probeer het opnieuw.";
  }
}
