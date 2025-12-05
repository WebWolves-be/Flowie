import { Component, inject, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { CommonModule } from "@angular/common";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { HttpErrorResponse } from "@angular/common/http";
import { Company } from "../../models/company.enum";
import { Project } from "../../models/project.model";
import { TaskFacade } from "../../task.facade";

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
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: "./save-project-dialog.component.html",
  styleUrl: "./save-project-dialog.component.scss"
})
export class SaveProjectDialogComponent {
  private ref = inject<DialogRef<SaveProjectDialogResult>>(DialogRef);
  private data = inject<SaveProjectDialogData>(DIALOG_DATA);
  private fb = inject(FormBuilder);
  private facade = inject(TaskFacade);

  readonly Company = Company;

  projectForm: FormGroup;

  readonly isUpdate = this.data.mode === "update";
  readonly isSaving = signal(false);
  readonly errorMessage = signal<string | null>(null);

  constructor() {
    this.projectForm = this.fb.group({
      title: [this.data.project?.title ?? "", Validators.required],
      description: [this.data.project?.description ?? ""],
      company: [this.data.project?.company ?? Company.Immoseed, Validators.required]
    });
  }

  get titleLabel(): string {
    return this.isUpdate ? "Project bewerken" : "Nieuw project aanmaken";
  }

  get actionLabel(): string {
    return this.isUpdate ? "Bewerken" : "Aanmaken";
  }

  onCancel(): void {
    this.ref.close();
  }

  onSubmit(): void {
    if (this.projectForm.invalid || this.isSaving()) {
      return;
    }

    this.errorMessage.set(null);
    this.isSaving.set(true);

    const formValue = this.projectForm.value;

    if (this.isUpdate && this.data.project) {
      this.facade.updateProject(this.data.project.projectId, {
        title: formValue.title,
        description: formValue.description,
        company: formValue.company
      }).subscribe({
        next: () => {
          this.isSaving.set(false);
          this.ref.close({ project: { ...this.data.project!, ...formValue }, mode: this.data.mode });
        },
        error: (error: HttpErrorResponse) => {
          this.isSaving.set(false);
          this.errorMessage.set(this.extractErrorMessage(error));
        }
      });
    } else {
      this.facade.createProject({
        title: formValue.title,
        description: formValue.description,
        company: formValue.company
      }).subscribe({
        next: () => {
          this.isSaving.set(false);
          const newProject: Project = {
            projectId: Date.now(),
            title: formValue.title,
            description: formValue.description,
            company: formValue.company,
            taskCount: 0,
            completedTaskCount: 0
          };
          this.ref.close({ project: newProject, mode: this.data.mode });
        },
        error: (error: HttpErrorResponse) => {
          this.isSaving.set(false);
          this.errorMessage.set(this.extractErrorMessage(error));
        }
      });
    }
  }

  private extractErrorMessage(error: HttpErrorResponse): string {
    if (error.status === 400 && error.error?.errors) {
      // FluentValidation error format
      const errors = error.error.errors;
      if (Array.isArray(errors) && errors.length > 0) {
        return errors.map((e: any) => e.errorMessage).join(' ');
      }
    }
    if (error.error?.title) {
      return error.error.title;
    }
    return 'Er is een fout opgetreden. Probeer het opnieuw.';
  }
}