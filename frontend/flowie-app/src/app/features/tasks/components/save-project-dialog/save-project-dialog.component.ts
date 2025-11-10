import { Component, inject, signal } from '@angular/core';
import { DialogRef, DIALOG_DATA } from '@angular/cdk/dialog';
import { CommonModule } from '@angular/common';
import { Company } from '../../models/company.enum';
import { ProjectDto } from '../../../../core/services/project-api.service';

export interface SaveProjectDialogData {
  mode: 'create' | 'update';
  project?: ProjectDto; // present when updating
}

export interface SaveProjectDialogResult {
  project: ProjectDto;
  mode: 'create' | 'update';
}

@Component({
  selector: 'app-save-project-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './save-project-dialog.component.html',
  styleUrl: './save-project-dialog.component.scss'
})
export class SaveProjectDialogComponent {
  private ref = inject<DialogRef<SaveProjectDialogResult>>(DialogRef);
  private data = inject<SaveProjectDialogData>(DIALOG_DATA);

  readonly Company = Company;

  title = signal(this.data.project?.title ?? '');
  description = signal(this.data.project?.description ?? '');
  company = signal<Company>(this.data.project?.company ?? Company.Immoseed);

  readonly isUpdate = this.data.mode === 'update';

  get titleLabel(): string {
    return this.isUpdate ? 'Project bewerken' : 'Nieuw project aanmaken';
  }

  get actionLabel(): string {
    return this.isUpdate ? 'Bewerken' : 'Aanmaken';
  }

  onCancel(): void {
    this.ref.close();
  }

  onSave(): void {
    // Preserve existing metrics when updating; initialize to zero on create.
    const base: ProjectDto = {
      id: this.data.project?.id ?? Date.now(), // temp id generation for mock
      title: this.title(),
      description: this.description() || null,
      company: this.company(),
      taskCount: this.data.project?.taskCount ?? 0,
      completedTaskCount: this.data.project?.completedTaskCount ?? 0,
      progress: this.data.project?.progress ?? 0
    };
    this.ref.close({ project: base, mode: this.data.mode });
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    this.onSave();
  }

  onNameInput(event: Event): void {
    this.title.set((event.target as HTMLInputElement).value);
  }

  onDescriptionInput(event: Event): void {
    this.description.set((event.target as HTMLTextAreaElement).value);
  }

  onCompanyChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value as Company;
    this.company.set(value);
  }
}