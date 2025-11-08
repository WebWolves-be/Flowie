import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DialogRef, DIALOG_DATA } from '@angular/cdk/dialog';
import { Task } from '../../models/task.model';
import { TaskStatus } from '../../models/task-status.enum';

export interface SaveTaskDialogData {
  mode: 'create' | 'update';
  projectId: number;
  task?: Task; // present when updating
}

export interface SaveTaskDialogResult {
  task: Task;
  mode: 'create' | 'update';
}

@Component({
  selector: 'app-save-task-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './save-task-dialog.component.html',
  styleUrl: './save-task-dialog.component.scss'
})
export class SaveTaskDialogComponent {
  private ref = inject<DialogRef<SaveTaskDialogResult>>(DialogRef);
  private data = inject<SaveTaskDialogData>(DIALOG_DATA);

  // Signals for form fields
  title = signal(this.data.task?.title ?? '');
  description = signal(this.data.task?.description ?? '');
  typeName = signal(this.data.task?.typeName ?? '');
  status = signal<TaskStatus>(this.data.task?.status ?? TaskStatus.Pending);
  dueDate = signal<string>(this.normalizeDate(this.data.task?.dueDate));
  assigneeName = signal(this.data.task?.assignee?.name ?? '');

  readonly isUpdate = this.data.mode === 'update';
  readonly TaskStatus = TaskStatus;

  titleLabel = computed(() => (this.isUpdate ? 'Taak bewerken' : 'Nieuwe taak aanmaken'));
  actionLabel = computed(() => (this.isUpdate ? 'Bewerken' : 'Aanmaken'));

  onCancel(): void {
    this.ref.close();
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    this.onSave();
  }

  onSave(): void {
    const base: Task = {
      id: this.data.task?.id ?? Date.now(), // temp id for mock
      projectId: this.data.projectId,
      parentTaskId: this.data.task?.parentTaskId ?? null,
      title: this.title(),
      description: this.description() || null,
      typeId: this.data.task?.typeId ?? undefined,
      typeName: this.typeName() || undefined,
      status: this.status(),
      statusName: this.statusToName(this.status()),
      dueDate: this.dueDate() || null,
      assignee: {
        id: this.data.task?.assignee?.id ?? null,
        name: this.assigneeName() || 'Onbekend',
        initials: this.getInitials(this.assigneeName() || 'Onbekend'),
        avatar: this.data.task?.assignee?.avatar
      },
      createdAt: this.data.task?.createdAt ?? new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      completedAt: this.status() === TaskStatus.Done ? new Date().toISOString() : null,
      subtaskCount: this.data.task?.subtaskCount ?? 0,
      completedSubtaskCount: this.data.task?.completedSubtaskCount ?? 0,
      progress: this.deriveProgress(this.status()),
      subtasks: this.data.task?.subtasks
    };
    this.ref.close({ task: base, mode: this.data.mode });
  }

  onTitleInput(e: Event) { this.title.set((e.target as HTMLInputElement).value); }
  onDescriptionInput(e: Event) { this.description.set((e.target as HTMLTextAreaElement).value); }
  onTypeNameInput(e: Event) { this.typeName.set((e.target as HTMLInputElement).value); }
  onAssigneeInput(e: Event) { this.assigneeName.set((e.target as HTMLInputElement).value); }
  onDueDateChange(e: Event) { this.dueDate.set((e.target as HTMLInputElement).value); }
  onStatusChange(e: Event) { this.status.set(Number((e.target as HTMLSelectElement).value) as TaskStatus); }

  private statusToName(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Done: return 'Done';
      case TaskStatus.Ongoing: return 'Ongoing';
      case TaskStatus.Pending:
      default: return 'Pending';
    }
  }

  private deriveProgress(status: TaskStatus): number {
    if (status === TaskStatus.Done) return 100;
    if (status === TaskStatus.Ongoing) return 33;
    return 0;
  }

  private getInitials(name: string): string {
    const parts = name.trim().split(/\s+/);
    if (parts.length === 0) return '';
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    return `${parts[0].charAt(0).toUpperCase()}${parts[parts.length - 1].charAt(0).toUpperCase()}`;
  }

  // Accepts ISO date, localized string, or undefined; returns yyyy-MM-dd or '' for <input type="date">
  private normalizeDate(input?: string | null): string {
    if (!input) return '';
    const iso = new Date(input);
    if (!isNaN(iso.getTime())) {
      const yyyy = iso.getFullYear();
      const mm = String(iso.getMonth() + 1).padStart(2, '0');
      const dd = String(iso.getDate()).padStart(2, '0');
      return `${yyyy}-${mm}-${dd}`;
    }
    // Fallback: leave as is; better to return empty to avoid invalid value for date input
    return '';
  }
}
