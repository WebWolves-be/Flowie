import { Component, computed, inject, OnInit, signal } from "@angular/core";
import { DIALOG_DATA, DialogRef } from "@angular/cdk/dialog";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Task } from "../../models/task.model";
import { TaskTypeFacade } from "../../../settings/facade/task-type.facade";
import { TaskFacade } from "../../task.facade";
import { EmployeeFacade } from "../../../../core/facades/employee.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { HttpErrorResponse } from "@angular/common/http";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";
import { catchError, EMPTY } from "rxjs";

export interface SaveTaskDialogData {
  mode: "create" | "update" | "create-subtask" | "update-subtask";
  projectId: number;
  task?: Task;
  parentTaskId?: number;
  parentTaskTitle?: string;
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
  #dialogRef = inject<DialogRef<SaveTaskDialogResult>>(DialogRef);
  #dialogData = inject<SaveTaskDialogData>(DIALOG_DATA);
  #taskTypeFacade = inject(TaskTypeFacade);
  #taskFacade = inject(TaskFacade);
  #employeeFacade = inject(EmployeeFacade);
  #notificationService = inject(NotificationService);

  #taskTypes = this.#taskTypeFacade.taskTypes;
  #employees = this.#employeeFacade.employees;

  readonly taskTypes = this.#taskTypes;
  readonly employees = this.#employees;

  errorMessage = signal<string | null>(null);

  taskForm!: FormGroup<{
    title: FormControl<string>;
    description: FormControl<string>;
    taskTypeId: FormControl<number | null>;
    dueDate: FormControl<string | null>;
    employeeId: FormControl<number | null>;
  }>;

  readonly isUpdate = this.#dialogData.mode === "update" || this.#dialogData.mode === "update-subtask";
  readonly isSubtask = this.#dialogData.mode === "create-subtask" || this.#dialogData.mode === "update-subtask";
  readonly isUpdateSubtask = this.#dialogData.mode === "update-subtask";

  ngOnInit(): void {
    const task = this.#dialogData.task;

    this.taskForm = new FormGroup({
      title: new FormControl(task?.title ?? "", { nonNullable: true, validators: [Validators.required] }),
      description: new FormControl(task?.description ?? "", { nonNullable: true }),
      taskTypeId: new FormControl<number | null>(task?.taskTypeId ?? null, { validators: [Validators.required] }),
      dueDate: new FormControl<string | null>(this.#normalizeDate(task?.dueDate)),
      employeeId: new FormControl<number | null>(task?.employeeId ?? null)
    });

    if (this.#taskTypes().length === 0) {
      this.#taskTypeFacade.getTaskTypes();
    }

    if (this.#employees().length === 0) {
      this.#employeeFacade.getEmployees();
    }

    if (this.isUpdate && this.hasSubtasks()) {
      this.#updateValidators();
    }
  }

  hasSubtasks = computed(() => {
    if (!this.isUpdate || !this.#dialogData.task) {
      return false;
    }

    const task = this.#dialogData.task;
    return (task.subtasks && task.subtasks.length > 0) || (task.subtaskCount && task.subtaskCount > 0);
  });

  titleLabel = computed(() => {
    if (this.isUpdateSubtask) return "Subtaak bewerken";
    if (this.isUpdate) return "Taak bewerken";
    if (this.isSubtask) return "Nieuwe subtaak aanmaken";
    return "Nieuwe taak aanmaken";
  });

  parentTaskTitle = computed(() => this.#dialogData.parentTaskTitle);

  actionLabel = computed(() => (this.isUpdate ? "Bewerken" : "Aanmaken"));

  get title() {
    return this.taskForm.get("title");
  }

  get dueDate() {
    return this.taskForm.get("dueDate");
  }

  onCancel(): void {
    this.#dialogRef.close();
  }

  onSubmit(): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.errorMessage.set(null);

    if (this.isUpdate) {
      this.#updateTask();
    } else {
      this.#createTask();
    }
  }

  #createTask(): void {
    const formValue = this.taskForm.value;

    const request = {
      projectId: this.#dialogData.projectId,
      title: formValue.title!,
      description: formValue.description || undefined,
      taskTypeId: formValue.taskTypeId!,
      dueDate: formValue.dueDate || undefined,
      employeeId: formValue.employeeId ?? undefined,
      parentTaskId: this.isSubtask ? this.#dialogData.parentTaskId : undefined
    };

    const successMessage = this.isSubtask ? "Subtaak succesvol aangemaakt" : "Taak succesvol aangemaakt";

    this.#taskFacade.createTask(request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.#taskFacade.getTasks(this.#dialogData.projectId);
        this.#taskFacade.getProjects();
        this.#notificationService.showSuccess(successMessage);
        this.#dialogRef.close();
      });
  }

  #updateTask(): void {
    const formValue = this.taskForm.value;
    const task = this.#dialogData.task!;

    const request = {
      title: formValue.title!,
      description: formValue.description || undefined,
      taskTypeId: this.hasSubtasks() ? task.taskTypeId : formValue.taskTypeId!,
      dueDate: this.hasSubtasks() ? (task.dueDate ?? undefined) : (formValue.dueDate || undefined),
      employeeId: this.hasSubtasks() ? (task.employeeId ?? undefined) : (formValue.employeeId ?? undefined),
      status: task.status
    };

    const successMessage = this.isUpdateSubtask ? "Subtaak succesvol bewerkt" : "Taak succesvol bewerkt";

    this.#taskFacade.updateTask(task.taskId, request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.#taskFacade.getTasks(this.#dialogData.projectId);
        this.#taskFacade.getProjects();
        this.#notificationService.showSuccess(successMessage);
        this.#dialogRef.close();
      });
  }


  #updateValidators(): void {
    const taskTypeControl = this.taskForm.get("taskTypeId");
    const dueDateControl = this.taskForm.get("dueDate");
    const employeeIdControl = this.taskForm.get("employeeId");

    if (this.hasSubtasks()) {
      taskTypeControl?.clearValidators();
      dueDateControl?.clearValidators();
      employeeIdControl?.clearValidators();
    } else {
      taskTypeControl?.setValidators([Validators.required]);
      dueDateControl?.clearValidators();
      employeeIdControl?.clearValidators();
    }

    taskTypeControl?.updateValueAndValidity();
    dueDateControl?.updateValueAndValidity();
    employeeIdControl?.updateValueAndValidity();
  }

  #normalizeDate(input?: string | null): string {
    if (!input) {
      return "";
    }

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
