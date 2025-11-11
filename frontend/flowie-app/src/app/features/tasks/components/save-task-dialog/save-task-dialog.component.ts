import { Component, computed, effect, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Task } from "../../models/task.model";
import { TaskStatus } from "../../models/task-status.enum";
import { TaskTypeFacade } from "../../../settings/facade/task-type.facade";
import { TaskFacade } from "../../task.facade";

export interface SaveTaskDialogData {
  mode: "create" | "update";
  projectId: number;
  task?: Task;
}

export interface SaveTaskDialogResult {
  task: Task;
  mode: "create" | "update";
}

@Component({
  selector: "app-save-task-dialog",
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: "./save-task-dialog.component.html",
  styleUrl: "./save-task-dialog.component.scss"
})
export class SaveTaskDialogComponent {
  private ref = inject<DialogRef<SaveTaskDialogResult>>(DialogRef);
  private data = inject<SaveTaskDialogData>(DIALOG_DATA);
  private taskTypeFacade = inject(TaskTypeFacade);
  private taskFacade = inject(TaskFacade);
  private fb = inject(FormBuilder);

  taskForm: FormGroup;

  taskTypes = this.taskTypeFacade.taskTypes;
  employees = this.taskFacade.employees;

  constructor() {
    this.taskTypeFacade.getTaskTypes();
    this.taskFacade.getEmployees();

    this.taskForm = this.fb.group({
      title: [this.data.task?.title ?? "", Validators.required],
      description: [this.data.task?.description ?? "", Validators.required],
      typeId: [this.data.task?.typeId ?? null, Validators.required],
      dueDate: [this.normalizeDate(this.data.task?.dueDate), Validators.required],
      assigneeId: [this.data.task?.employeeId ?? null, Validators.required]
    });

    if (this.isUpdate && this.data.task?.employeeName && !this.data.task?.employeeId) {
      effect(() => {
        const employees = this.employees();
        const taskAssigneeName = this.data.task?.employeeName;
        if (employees.length > 0 && taskAssigneeName) {
          const employee = employees.find(e => e.name === taskAssigneeName);
          if (employee) {
            this.taskForm.patchValue({ assigneeId: employee.id });
          }
        }
      });
    }
  }

  readonly isUpdate = this.data.mode === "update";
  readonly TaskStatus = TaskStatus;

  hasSubtasks = computed(() => {
    const task = this.data.task;
    return this.isUpdate && task && ((task.subtasks && task.subtasks.length > 0) || (task.subtaskCount && task.subtaskCount > 0));
  });

  titleLabel = computed(() => (this.isUpdate ? "Taak bewerken" : "Nieuwe taak aanmaken"));
  actionLabel = computed(() => (this.isUpdate ? "Bewerken" : "Aanmaken"));

  onCancel(): void {
    this.ref.close();
  }

  onSubmit(): void {
    if (this.taskForm.invalid) {
      return;
    }

    const hasSubtasksValue = this.hasSubtasks();
    const formValue = this.taskForm.value;

    const typeId = hasSubtasksValue ? this.data.task?.typeId! : formValue.typeId;
    const selectedType = this.taskTypes().find(t => t.id === typeId);

    const employeeId = hasSubtasksValue 
      ? (this.data.task?.employeeId ?? this.employees().find(e => e.name === this.data.task?.employeeName)?.id)
      : formValue.assigneeId;
    const selectedEmployee = this.employees().find(e => e.id === employeeId);

    const status = this.data.task?.status ?? TaskStatus.Pending;

    const base: Task = {
      taskId: this.data.task?.taskId ?? Date.now(),
      projectId: this.data.projectId,
      title: formValue.title,
      description: formValue.description || null,
      typeId: typeId,
      typeName: selectedType?.name ?? "",
      status: status,
      dueDate: hasSubtasksValue ? this.data.task?.dueDate! : formValue.dueDate,
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

  private normalizeDate(input?: string | null): string {
    if (!input) return "";
    const iso = new Date(input);
    if (!isNaN(iso.getTime())) {
      const yyyy = iso.getFullYear();
      const mm = String(iso.getMonth() + 1).padStart(2, "0");
      const dd = String(iso.getDate()).padStart(2, "0");
      return `${yyyy}-${mm}-${dd}`;
    }
    return "";
  }
}
