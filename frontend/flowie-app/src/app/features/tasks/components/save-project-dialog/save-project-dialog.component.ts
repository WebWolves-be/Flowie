import { Component, inject, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { catchError, EMPTY } from "rxjs";
import { Company } from "../../models/company.enum";
import { Project } from "../../models/project.model";
import { TaskFacade } from "../../task.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";

export interface SaveProjectDialogData {
  mode: "create" | "update";
  project?: Project;
}

export interface SaveProjectDialogResult {
  project: Project;
  mode: "create" | "update";
}

@Component({
  selector: "app-save-project-dialog",
  standalone: true,
  imports: [ReactiveFormsModule],
  templateUrl: "./save-project-dialog.component.html",
  styleUrl: "./save-project-dialog.component.scss"
})
export class SaveProjectDialogComponent {
  #dialogRef = inject(DialogRef);
  #dialogData = inject<SaveProjectDialogData>(DIALOG_DATA);
  #facade = inject(TaskFacade);
  #notificationService = inject(NotificationService);

  readonly Company = Company;
  readonly isUpdate = this.#dialogData.mode === "update";

  form = new FormGroup({
    title: new FormControl(
      this.#dialogData.project?.title?.trim() ?? "",
      { validators: [Validators.required], nonNullable: true }
    ),
    description: new FormControl(
      this.#dialogData.project?.description ?? "",
      { nonNullable: true }
    ),
    company: new FormControl(
      this.#dialogData.project?.company ?? Company.Immoseed,
      { validators: [Validators.required], nonNullable: true }
    )
  });

  errorMessage = signal<string | null>(null);

  get titleLabel(): string {
    return this.isUpdate ? "Project bewerken" : "Nieuw project aanmaken";
  }

  get actionLabel(): string {
    return this.isUpdate ? "Bewerken" : "Aanmaken";
  }

  onCancel(): void {
    this.#dialogRef.close();
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const trimmedTitle = this.form.value.title!.trim();

    if (!trimmedTitle) {
      this.form.controls.title.setErrors({ required: true });
      this.form.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);

    const trimmedDescription = this.form.value.description!.trim();

    if (this.isUpdate && this.#dialogData.project) {
      this.#facade.updateProject(this.#dialogData.project.projectId, {
        title: trimmedTitle,
        description: trimmedDescription || undefined,
        company: this.form.value.company!
      })
        .pipe(
          catchError((error: HttpErrorResponse) => {
            this.errorMessage.set(extractErrorMessage(error));
            return EMPTY;
          })
        )
        .subscribe(() => {
          this.#facade.getProjects();
          this.#notificationService.showSuccess("Project succesvol bijgewerkt");
          this.#dialogRef.close();
        });
    } else {
      this.#facade.createProject({
        title: trimmedTitle,
        description: trimmedDescription || undefined,
        company: this.form.value.company!
      })
        .pipe(
          catchError((error: HttpErrorResponse) => {
            this.errorMessage.set(extractErrorMessage(error));
            return EMPTY;
          })
        )
        .subscribe(() => {
          this.#facade.getProjects();
          this.#notificationService.showSuccess("Project succesvol aangemaakt");
          this.#dialogRef.close();
        });
    }
  }
}