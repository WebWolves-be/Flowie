import { Component, computed, DestroyRef, inject, OnInit, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import { ProjectListComponent } from "../project-list/project-list.component";
import { ProjectDetailComponent } from "../project-detail/project-detail.component";
import { TaskFacade } from "../../task.facade";
import { Company } from "../../models/company.enum";
import { TaskStatus } from "../../models/task-status.enum";
import { Dialog } from "@angular/cdk/dialog";
import {
  SaveProjectDialogComponent,
  SaveProjectDialogData,
  SaveProjectDialogResult
} from "../save-project-dialog/save-project-dialog.component";
import {
  SaveTaskDialogComponent,
  SaveTaskDialogData,
  SaveTaskDialogResult
} from "../save-task-dialog/save-task-dialog.component";
import { DeleteTaskDialogComponent } from "../delete-task-dialog/delete-task-dialog.component";
import { DeleteTaskDialogData } from "../../models/delete-task-dialog-data.model";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { TaskTypeFacade } from "../../../settings/facade/task-type.facade";
import { EmployeeFacade } from "../../../../core/facades/employee.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { catchError, EMPTY } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";

@Component({
  selector: "app-tasks-page",
  standalone: true,
  imports: [CommonModule, ProjectListComponent, ProjectDetailComponent],
  templateUrl: "./tasks-page.html",
  styleUrl: "./tasks-page.scss"
})
export class TasksPage implements OnInit {
  #taskFacade = inject(TaskFacade);
  #taskTypeFacade = inject(TaskTypeFacade);
  #employeeFacade = inject(EmployeeFacade);
  #notificationService = inject(NotificationService);
  #router = inject(Router);
  #route = inject(ActivatedRoute);
  #dialog = inject(Dialog);
  #destroy = inject(DestroyRef);

  #hasInitiallyLoaded = false;

  readonly Company = Company;

  projects = this.#taskFacade.projects;
  isLoadingProjects = this.#taskFacade.isLoadingProjects;

  tasks = this.#taskFacade.tasks;
  isLoadingTasks = this.#taskFacade.isLoadingTasks;

  companyFilter = this.#taskFacade.companyFilter;

  selectedProjectId = signal<number | null>(null);
  showOnlyMyTasks = signal<boolean>(false);

  selectedProject = computed(() => {
    const id = this.selectedProjectId();
    return id ? this.projects().find(p => p.projectId === id) : undefined;
  });

  ngOnInit(): void {
    if (this.projects().length === 0 && !this.#hasInitiallyLoaded) {
      this.#hasInitiallyLoaded = true;
      this.#taskFacade.getProjects();
    }

    if (this.#taskTypeFacade.taskTypes().length === 0) {
      this.#taskTypeFacade.getTaskTypes();
    }

    if (this.#employeeFacade.employees().length === 0) {
      this.#employeeFacade.getEmployees();
    }

    this.#route.paramMap.pipe(takeUntilDestroyed(this.#destroy)).subscribe(params => {
      const projectId = params.get("id");

      if (projectId) {
        const idNum = Number(projectId);

        this.selectedProjectId.set(idNum);
        this.showOnlyMyTasks.set(false);

        this.#taskFacade.getTasks(idNum, false);
      } else {
        this.#taskFacade.clearTasks();

        this.selectedProjectId.set(null);
      }
    });
  }

  onProjectSelected(projectId: number) {
    void this.#router.navigate(["/taken/project", projectId]);
  }

  onFilterProjectsByCompany(filter: "ALL" | Company) {
    this.selectedProjectId.set(null);
    this.#taskFacade.setCompanyFilter(filter);

    void this.#router.navigate(["/taken"]);
  }

  onOpenCreateProjectDialog() {
    this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: "create" } as SaveProjectDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  onOpenUpdateProjectDialog() {
    const proj = this.selectedProject();

    if (!proj) {
      return;
    }

    this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: "update", project: proj } as SaveProjectDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  onTaskFilterChanges(showOnlyMyTasks: boolean) {
    this.showOnlyMyTasks.set(showOnlyMyTasks);
    const selectedProjectId = this.selectedProjectId();

    if (selectedProjectId) {
      this.#taskFacade.getTasks(selectedProjectId, showOnlyMyTasks);
    }
  }

  onOpenCreateTaskDialog() {
    const selectedProject = this.selectedProject();

    if (!selectedProject) {
      return;
    }

    this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "create", projectId: selectedProject.projectId } as SaveTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  onOpenUpdateTaskDialog(taskId: number) {
    const task = this.tasks().find(t => t.taskId === taskId);
    const selectedProject = this.selectedProject();

    if (!task || !selectedProject) {
      return;
    }

    this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "update", projectId: selectedProject.projectId, task } as SaveTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  onOpenDeleteTaskDialog(taskId: number) {
    const task = this.tasks().find(t => t.taskId === taskId);

    if (!task) {
      return;
    }

    const dialogRef = this.#dialog.open<boolean>(DeleteTaskDialogComponent, {
      data: { task } as DeleteTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });

    dialogRef.closed.subscribe(result => {
      if (result) {
        const projectId = this.selectedProjectId();
        if (projectId) {
          this.#taskFacade.getTasks(projectId, this.showOnlyMyTasks());
          this.#taskFacade.getProjects();
        }
      }
    });
  }

  onTaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    const statusMessages = {
      [TaskStatus.Ongoing]: "Taak gestart",
      [TaskStatus.Done]: "Taak voltooid",
      [TaskStatus.Pending]: "Taak heropend"
    };

    this.#taskFacade
      .updateTaskStatus(event.taskId, { status: event.status })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.#notificationService.showError(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        const projectId = this.selectedProjectId();
        if (projectId) {
          this.#taskFacade.getTasks(projectId, this.showOnlyMyTasks());
          this.#taskFacade.getProjects();
        }
        this.#notificationService.showSuccess(statusMessages[event.status]);
      });
  }

  onSubtaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    const statusMessages = {
      [TaskStatus.Ongoing]: "Subtaak gestart",
      [TaskStatus.Done]: "Subtaak voltooid",
      [TaskStatus.Pending]: "Subtaak heropend"
    };

    this.#taskFacade
      .updateTaskStatus(event.taskId, { status: event.status })
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.#notificationService.showError(extractErrorMessage(error));
          return EMPTY;
        })
      )
      .subscribe(() => {
        const projectId = this.selectedProjectId();
        if (projectId) {
          this.#taskFacade.getTasks(projectId, this.showOnlyMyTasks());
          this.#taskFacade.getProjects();
        }
        this.#notificationService.showSuccess(statusMessages[event.status]);
      });
  }

  onOpenCreateSubtaskDialog(parentTaskId: number) {
    const selectedProject = this.selectedProject();

    if (!selectedProject) {
      return;
    }

    const parentTask = this.#taskFacade.tasks().find(t => t.taskId === parentTaskId);

    this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: {
        mode: "create-subtask",
        projectId: selectedProject.projectId,
        parentTaskId,
        parentTaskTitle: parentTask?.title
      } as SaveTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  onOpenUpdateSubtaskDialog(subtaskId: number) {
    const selectedProject = this.selectedProject();

    if (!selectedProject) {
      return;
    }

    let subtask = null;
    for (const task of this.tasks()) {
      if (task.subtasks) {
        subtask = task.subtasks.find(s => s.taskId === subtaskId);
        if (subtask) break;
      }
    }

    if (!subtask) {
      return;
    }

    const taskData = {
      taskId: subtask.taskId,
      projectId: selectedProject.projectId,
      title: subtask.title,
      description: subtask.description,
      taskTypeId: subtask.taskTypeId,
      taskTypeName: subtask.taskTypeName,
      dueDate: subtask.dueDate,
      status: subtask.status,
      employeeId: subtask.employeeId,
      employeeName: subtask.employeeName,
      createdAt: subtask.createdAt,
      updatedAt: subtask.updatedAt,
      completedAt: subtask.completedAt,
      subtaskCount: 0,
      completedSubtaskCount: 0,
      subtasks: []
    };

    this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "update-subtask", projectId: selectedProject.projectId, task: taskData } as SaveTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  onOpenDeleteSubtaskDialog(subtaskId: number) {
    let subtask = null;
    for (const task of this.tasks()) {
      if (task.subtasks) {
        subtask = task.subtasks.find(s => s.taskId === subtaskId);
        if (subtask) break;
      }
    }

    if (!subtask) {
      return;
    }

    const taskData = {
      taskId: subtask.taskId,
      projectId: this.selectedProject()!.projectId,
      title: subtask.title,
      description: subtask.description,
      taskTypeId: subtask.taskTypeId,
      taskTypeName: subtask.taskTypeName,
      dueDate: subtask.dueDate,
      status: subtask.status,
      employeeId: subtask.employeeId,
      employeeName: subtask.employeeName,
      createdAt: subtask.createdAt,
      updatedAt: subtask.updatedAt,
      completedAt: subtask.completedAt,
      subtaskCount: 0,
      completedSubtaskCount: 0,
      subtasks: []
    };

    const dialogRef = this.#dialog.open<boolean>(DeleteTaskDialogComponent, {
      data: { task: taskData, isSubtask: true } as DeleteTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });

    dialogRef.closed.subscribe(result => {
      if (result) {
        const projectId = this.selectedProjectId();
        if (projectId) {
          this.#taskFacade.getTasks(projectId, this.showOnlyMyTasks());
          this.#taskFacade.getProjects();
        }
      }
    });
  }
}
