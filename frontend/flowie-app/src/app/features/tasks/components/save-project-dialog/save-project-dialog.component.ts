import { Component, inject, OnInit } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { CommonModule } from "@angular/common";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Company } from "../../models/company.enum";
import { Project } from "../../models/project.model";

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
export class SaveProjectDialogComponent implements OnInit {
  private ref = inject<DialogRef<SaveProjectDialogResult>>(DialogRef);
  private data = inject<SaveProjectDialogData>(DIALOG_DATA);
  private fb = inject(FormBuilder);

  readonly Company = Company;

  projectForm!: FormGroup;

  readonly isUpdate = this.data.mode === "update";

  ngOnInit(): void {
    this.projectForm = this.fb.group({
      title: [
        this.data.project?.title ?? "",
        [
          Validators.required,
          Validators.minLength(3),
          Validators.maxLength(200)
        ]
      ],
      description: [
        this.data.project?.description ?? "",
        [Validators.maxLength(4000)]
      ],
      company: [this.data.project?.company ?? Company.Immoseed, Validators.required]
    });
  }

  get title() {
    return this.projectForm.get("title");
  }

  get description() {
    return this.projectForm.get("description");
  }

  get titleLabel(): string {
    return this.isUpdate ? "ProjectModel bewerken" : "Nieuw project aanmaken";
  }

  get actionLabel(): string {
    return this.isUpdate ? "Bewerken" : "Aanmaken";
  }

  onCancel(): void {
    this.ref.close();
  }

  onSubmit(): void {
    if (this.projectForm.invalid) {
      return;
    }

    const formValue = this.projectForm.value;
    const base: Project = {
      projectId: this.data.project?.projectId ?? Date.now(),
      title: formValue.title,
      description: formValue.description,
      company: formValue.company,
      taskCount: this.data.project?.taskCount ?? 0,
      completedTaskCount: this.data.project?.completedTaskCount ?? 0
    };
    this.ref.close({ project: base, mode: this.data.mode });
  }
}