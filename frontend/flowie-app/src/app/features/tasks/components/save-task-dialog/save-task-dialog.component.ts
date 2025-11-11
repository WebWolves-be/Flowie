import { Component, computed, effect, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { Task } from "../../models/task.model";
import { TaskStatus } from "../../models/task-status.enum";
import { TaskTypeFacade } from "../../../settings/facade/task-type.facade";
import { TaskFacade } from "../../task.facade";

export interface SaveTaskDialogData {
  mode: "create" | "update";
  projectId: number;
  task?: Task; // present when updating
}

export interface SaveTaskDialogResult {
  task: Task;
  mode: "create" | "update";
}

@Component({
  selector: "app-save-task-dialog",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./save-task-dialog.component.html",
  styleUrl: "./save-task-dialog.component.scss"
})
export class SaveTaskDialogComponent {
  private ref = inject<DialogRef<SaveTaskDialogResult>>(DialogRef);
  private data = inject<SaveTaskDialogData>(DIALOG_DATA);
  private taskTypeFacade = inject(TaskTypeFacade);
  private taskFacade = inject(TaskFacade);

  // Signals for form fields
  title = signal(this.data.task?.title ?? "");
  description = signal(this.data.task?.description ?? "");
  typeId = signal<number | undefined>(this.data.task?.typeId);
  dueDate = signal<string>(this.normalizeDate(this.data.task?.dueDate));
  assigneeId = signal<number | undefined>(this.data.task?.employeeId ?? undefined);

  // Exposed data from facades
  taskTypes = this.taskTypeFacade.taskTypes;
  employees = this.taskFacade.employees;

  constructor() {
    // Load data on init
    this.taskTypeFacade.getTaskTypes();
    this.taskFacade.getEmployees();

    // When editing and assignee doesn't have an ID, find it by name once employees load
    if (this.isUpdate && this.data.task?.employeeName && !this.assigneeId()) {
      effect(() => {
        const employees = this.employees();
        const taskAssigneeName = this.data.task?.employeeName;
        if (employees.length > 0 && taskAssigneeName) {
          const employee = employees.find(e => e.name === taskAssigneeName);
          if (employee) {
            this.assigneeId.set(employee.id);
          }
        }
      });
    }
  }

  readonly isUpdate = this.data.mode === "update";
  readonly TaskStatus = TaskStatus;

  // Check if task has subtasks (when editing, only allow title and description)
  hasSubtasks = computed(() => {
    const task = this.data.task;
    return this.isUpdate && task && ((task.subtasks && task.subtasks.length > 0) || (task.subtaskCount && task.subtaskCount > 0));
  });

  titleLabel = computed(() => (this.isUpdate ? "Taak bewerken" : "Nieuwe taak aanmaken"));
  actionLabel = computed(() => (this.isUpdate ? "Bewerken" : "Aanmaken"));

  onCancel(): void {
    this.ref.close();
  }

  onSubmit(event: Event): void {
    event.preventDefault();
    this.onSave();
  }

  onSave(): void {
    // If task has subtasks, preserve existing values for type, deadline, and assignee
    const hasSubtasksValue = this.hasSubtasks();

    const selectedType = hasSubtasksValue
      ? this.taskTypes().find(t => t.id === this.data.task?.typeId)
      : this.taskTypes().find(t => t.id === this.typeId());

    const selectedEmployee = hasSubtasksValue
      ? this.employees().find(e => e.id === this.data.task?.employeeId || e.name === this.data.task?.employeeName)
      : this.employees().find(e => e.id === this.assigneeId());

    // Use existing status when updating, default to Pending when creating
    const status = this.data.task?.status ?? TaskStatus.Pending;

    const base: Task = {
      taskId: this.data.task?.taskId ?? Date.now(), // temp id for mock
      projectId: this.data.projectId,
      title: this.title(),
      description: this.description() || null,
      typeId: hasSubtasksValue ? this.data.task?.typeId! : this.typeId()!,
      typeName: selectedType?.name ?? "",
      status: status,
      dueDate: hasSubtasksValue ? this.data.task?.dueDate! : this.dueDate(),
      employeeId: selectedEmployee!.id,
      employeeName: selectedEmployee!.name,
      createdAt: this.data.task?.createdAt ?? new Date().toISOString(),
      updatedAt: this.data.task?.updatedAt ?? null,
      completedAt: status === TaskStatus.Done ? new Date().toISOString() : null,
      subtaskCount: this.data.task?.subtaskCount ?? 0,
      completedSubtaskCount: this.data.task?.completedSubtaskCount ?? 0,
      subtasks: this.data.task?.subtasks ?? []
    };
    this.ref.close({ task: base, mode: this.data.mode });
  }

  onTitleInput(e: Event) {
    this.title.set((e.target as HTMLInputElement).value);
  }

  onDescriptionInput(e: Event) {
    this.description.set((e.target as HTMLTextAreaElement).value);
  }

  onTypeChange(e: Event) {
    this.typeId.set(Number((e.target as HTMLSelectElement).value) || undefined);
  }

  onAssigneeChange(e: Event) {
    this.assigneeId.set(Number((e.target as HTMLSelectElement).value) || undefined);
  }

  onDueDateChange(e: Event) {
    this.dueDate.set((e.target as HTMLInputElement).value);
  }

  private statusToName(status: TaskStatus): string {
    switch (status) {
      case TaskStatus.Done:
        return "Done";
      case TaskStatus.Ongoing:
        return "Ongoing";
      case TaskStatus.Pending:
      default:
        return "Pending";
    }
  }

  private deriveProgress(status: TaskStatus): number {
    if (status === TaskStatus.Done) return 100;
    if (status === TaskStatus.Ongoing) return 33;
    return 0;
  }

  // Accepts ISO date, localized string, or undefined; returns yyyy-MM-dd or '' for <input type="date">
  private normalizeDate(input?: string | null): string {
    if (!input) return "";
    const iso = new Date(input);
    if (!isNaN(iso.getTime())) {
      const yyyy = iso.getFullYear();
      const mm = String(iso.getMonth() + 1).padStart(2, "0");
      const dd = String(iso.getDate()).padStart(2, "0");
      return `${yyyy}-${mm}-${dd}`;
    }
    // Fallback: leave as is; better to return empty to avoid invalid value for date input
    return "";
  }
}
