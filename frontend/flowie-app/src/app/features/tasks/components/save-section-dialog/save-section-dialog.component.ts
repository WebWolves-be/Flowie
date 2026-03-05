import { Component, inject, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { catchError, EMPTY } from "rxjs";
import { QuillEditorComponent } from "ngx-quill";
import { Section } from "../../models/section.model";
import { TaskFacade } from "../../task.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";

export interface SaveSectionDialogData {
  mode: "create" | "update";
  projectId: number;
  section?: Section;
}

@Component({
  selector: "app-save-section-dialog",
  standalone: true,
  imports: [ReactiveFormsModule, QuillEditorComponent],
  templateUrl: "./save-section-dialog.component.html",
  styleUrl: "./save-section-dialog.component.scss"
})
export class SaveSectionDialogComponent {
  #dialogRef = inject(DialogRef);
  #dialogData = inject<SaveSectionDialogData>(DIALOG_DATA);
  #facade = inject(TaskFacade);
  #notificationService = inject(NotificationService);

  readonly isUpdate = this.#dialogData.mode === "update";

  editorModules = {
    toolbar: [
      ['bold', 'italic'],
      [{ list: 'ordered' }, { list: 'bullet' }]
    ]
  };

  form = new FormGroup({
    title: new FormControl(
      this.#dialogData.section?.title?.trim() ?? "",
      { validators: [Validators.required], nonNullable: true }
    ),
    description: new FormControl(
      this.#dialogData.section?.description ?? "",
      { nonNullable: true }
    )
  });

  errorMessage = signal<string | null>(null);

  get titleLabel(): string {
    return this.isUpdate ? "Sectie bewerken" : "Nieuwe sectie aanmaken";
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

    if (this.isUpdate && this.#dialogData.section) {
      this.#facade.updateSection(this.#dialogData.section.sectionId, {
        title: trimmedTitle,
        description: trimmedDescription || undefined
      })
        .pipe(
          catchError((error: HttpErrorResponse) => {
            this.errorMessage.set(extractErrorMessage(error));
            return EMPTY;
          })
        )
        .subscribe(() => {
          this.#facade.getSections(this.#dialogData.projectId);
          this.#notificationService.showSuccess("Sectie succesvol bewerkt");
          this.#dialogRef.close();
        });
    } else {
      this.#facade.createSection({
        projectId: this.#dialogData.projectId,
        title: trimmedTitle,
        description: trimmedDescription || undefined
      })
        .pipe(
          catchError((error: HttpErrorResponse) => {
            this.errorMessage.set(extractErrorMessage(error));
            return EMPTY;
          })
        )
        .subscribe(() => {
          this.#facade.getSections(this.#dialogData.projectId);
          this.#notificationService.showSuccess("Sectie succesvol aangemaakt");
          this.#dialogRef.close();
        });
    }
  }
}
