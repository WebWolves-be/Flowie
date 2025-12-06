import { Component, computed, effect, inject, OnInit, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Task } from "../../models/task.model";
import { TaskStatus } from "../../models/task-status.enum";
import { TaskTypeFacade } from "../../../settings/facade/task-type.facade";
import { TaskFacade } from "../../task.facade";
import { EmployeeFacade } from "../../../../core/facades/employee.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { HttpErrorResponse } from "@angular/common/http";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";
import { catchError, EMPTY } from "rxjs";

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
  imports: [ReactiveFormsModule],
  templateUrl: "./save-task-dialog.component.html",
  styleUrl: "./save-task-dialog.component.scss"
})
export class SaveTaskDialogComponent implements OnInit {
  #ref = inject<DialogRef<SaveTaskDialogResult>>(DialogRef);
  #data = inject<SaveTaskDialogData>(DIALOG_DATA);
  #taskTypeFacade = inject(TaskTypeFacade);
  #taskFacade = inject(TaskFacade);
  #employeeFacade = inject(EmployeeFacade);
  #notificationService = inject(NotificationService);

  taskForm!: FormGroup<{
    title: FormControl<string>;
    description: FormControl<string>;
    taskTypeId: FormControl<number | null>;
    dueDate: FormControl<string>;
    employeeId: FormControl<number | null>;
  }>;

  errorMessage = signal<string | null>(null);

  readonly taskTypes = this.#taskTypeFacade.taskTypes;
  readonly employees = this.#employeeFacade.employees;

  ngOnInit(): void {
    this.#taskTypeFacade.getTaskTypes();
    this.#employeeFacade.getEmployees();

    this.taskForm = new FormGroup({
      title: new FormControl(this.#data.task?.title ?? "", { nonNullable: true, validators: [Validators.required] }),
      description: new FormControl(this.#data.task?.description ?? "", { nonNullable: true }),
      taskTypeId: new FormControl<number | null>(this.#data.task?.typeId ?? null, { validators: [Validators.required] }),
      dueDate: new FormControl(this.normalizeDate(this.#data.task?.dueDate), { nonNullable: true, validators: [Validators.required] }),
      employeeId: new FormControl<number | null>(this.#data.task?.employeeId ?? null, { validators: [Validators.required] })
    });

    if (this.isUpdate && this.#data.task?.employeeName && !this.#data.task?.employeeId) {
      effect(() => {
        const employees = this.employees();
        const taskAssigneeName = this.#data.task?.employeeName;
        if (employees.length > 0 && taskAssigneeName) {
          const employee = employees.find(e => e.name === taskAssigneeName);
          if (employee) {
            this.taskForm.patchValue({ employeeId: employee.id });
          }
        }
      });
    }
  }

  readonly isUpdate = this.#data.mode === "update";
  readonly TaskStatus = TaskStatus;

  hasSubtasks = computed(() => {
    const task = this.#data.task;
    return this.isUpdate && task && ((task.subtasks && task.subtasks.length > 0) || (task.subtaskCount && task.subtaskCount > 0));
  });

  titleLabel = computed(() => (this.isUpdate ? "Taak bewerken" : "Nieuwe taak aanmaken"));
  actionLabel = computed(() => (this.isUpdate ? "Bewerken" : "Aanmaken"));

  get title() {
    return this.taskForm.get("title");
  }

  get description() {
    return this.taskForm.get("description");
  }

  get dueDate() {
    return this.taskForm.get("dueDate");
  }

  onCancel(): void {
    this.#ref.close();
  }

  onSubmit(): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);

    const hasSubtasksValue = this.hasSubtasks();
    const formValue = this.taskForm.value;

    if (this.isUpdate) {
      const typeId = hasSubtasksValue ? this.#data.task?.typeId! : formValue.taskTypeId!;
      const employeeId = hasSubtasksValue
        ? (this.#data.task?.employeeId ?? this.employees().find(e => e.name === this.#data.task?.employeeName)?.id)
        : formValue.employeeId;

      if (!employeeId) {
        console.error('No employee found');
        return;
      }

      const status = this.#data.task?.status ?? TaskStatus.Pending;
      const request = {
        title: formValue.title!,
        description: formValue.description || undefined,
        typeId: typeId,
        dueDate: hasSubtasksValue ? this.#data.task?.dueDate! : formValue.dueDate!,
        employeeId: employeeId,
        status: status,
        progress: this.calculateProgress(this.#data.task!)
      };

      this.#taskFacade.updateTask(this.#data.task!.taskId, request)
        .pipe(
          catchError((error: HttpErrorResponse) => {
            this.errorMessage.set(extractErrorMessage(error));
            return EMPTY;
          })
        )
        .subscribe(() => {
          this.#taskFacade.getTasks(this.#data.projectId);
          this.#notificationService.showSuccess("Taak succesvol bewerkt");
          this.#ref.close();
        });
    } else {
      const request = {
        projectId: this.#data.projectId,
        title: formValue.title!,
        description: formValue.description || undefined,
        taskTypeId: formValue.taskTypeId!,
        dueDate: formValue.dueDate!,
        employeeId: formValue.employeeId!
      };

      this.#taskFacade.createTask(request)
        .pipe(
          catchError((error: HttpErrorResponse) => {
            this.errorMessage.set(extractErrorMessage(error));
            return EMPTY;
          })
        )
        .subscribe(() => {
          this.#taskFacade.getTasks(this.#data.projectId);
          this.#notificationService.showSuccess("Taak succesvol aangemaakt");
          this.#ref.close();
        });
    }
  }

  private calculateProgress(task: Task): number {
    if (!task.subtaskCount || task.subtaskCount === 0) {
      return task.status === TaskStatus.Done ? 100 : 0;
    }
    return Math.round(((task.completedSubtaskCount ?? 0) / task.subtaskCount) * 100);
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
