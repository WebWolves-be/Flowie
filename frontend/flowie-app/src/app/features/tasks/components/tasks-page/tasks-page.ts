import { Component, computed, DestroyRef, effect, inject, OnInit, signal } from "@angular/core";
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
  SaveSectionDialogComponent,
  SaveSectionDialogData
} from "../save-section-dialog/save-section-dialog.component";
import { DeleteSectionDialogComponent } from "../delete-section-dialog/delete-section-dialog.component";
import { DeleteSectionDialogData } from "../../models/delete-section-dialog-data.model";
import {
  SaveTaskDialogComponent,
  SaveTaskDialogData,
  SaveTaskDialogResult
} from "../save-task-dialog/save-task-dialog.component";
import { DeleteTaskDialogComponent } from "../delete-task-dialog/delete-task-dialog.component";
import { DeleteTaskDialogData } from "../../models/delete-task-dialog-data.model";
import { DeleteProjectDialogComponent } from "../delete-project-dialog/delete-project-dialog.component";
import { DeleteProjectDialogData } from "../../models/delete-project-dialog-data.model";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { TaskTypeFacade } from "../../../settings/facade/task-type.facade";
import { EmployeeFacade } from "../../../../core/facades/employee.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { BreakpointService } from "../../../../core/services/breakpoint.service";
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
  #breakpointService = inject(BreakpointService);
  #router = inject(Router);
  #route = inject(ActivatedRoute);
  #dialog = inject(Dialog);
  #destroy = inject(DestroyRef);

  #hasInitiallyLoaded = false;
  #loadingTimer: ReturnType<typeof setTimeout> | null = null;

  readonly Company = Company;

  projects = computed(() =>
    [...this.#taskFacade.projects()].sort((a, b) => {
      const aDone = a.taskCount > 0 && a.taskCount === a.completedTaskCount;
      const bDone = b.taskCount > 0 && b.taskCount === b.completedTaskCount;
      if (aDone !== bDone) return aDone ? 1 : -1;
      return 0;
    })
  );
  isLoadingProjects = this.#taskFacade.isLoadingProjects;

  sections = computed(() =>
    [...this.#taskFacade.sections()].sort((a, b) => {
      const aDone = a.taskCount > 0 && a.taskCount === a.completedTaskCount;
      const bDone = b.taskCount > 0 && b.taskCount === b.completedTaskCount;
      if (aDone !== bDone) return aDone ? 1 : -1;
      return a.displayOrder - b.displayOrder;
    })
  );
  isLoadingSections = this.#taskFacade.isLoadingSections;

  tasks = this.#taskFacade.tasks;
  #facadeIsLoadingTasks = this.#taskFacade.isLoadingTasks;
  isLoadingTasks = signal<boolean>(false);

  companyFilter = this.#taskFacade.companyFilter;

  selectedProjectId = signal<number | null>(null);
  showOnlyMyTasks = signal<boolean>(false);
  mobileView = signal<'list' | 'detail'>('list');

  isMobile = this.#breakpointService.isMobile;

  selectedProject = computed(() => {
    const id = this.selectedProjectId();
    return id ? this.projects().find(p => p.projectId === id) : undefined;
  });

  showProjectList = computed(() => {
    if (!this.isMobile()) return true;
    return this.mobileView() === 'list';
  });

  showProjectDetail = computed(() => {
    if (!this.isMobile()) return true;
    return this.mobileView() === 'detail' && this.selectedProjectId() !== null;
  });

  constructor() {
    // Watch for facade loading state changes
    effect(() => {
      const facadeLoading = this.#facadeIsLoadingTasks();

      if (!facadeLoading) {
        // API finished - clear timer and hide loading state
        if (this.#loadingTimer) {
          clearTimeout(this.#loadingTimer);
          this.#loadingTimer = null;
        }
        this.isLoadingTasks.set(false);
      }
    });
  }

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

        this.#taskFacade.getSections(idNum);
        this.#loadTasksWithDelay(idNum, false);
      } else {
        this.#taskFacade.clearSections();
        this.#taskFacade.clearTasks();

        this.selectedProjectId.set(null);
      }
    });
  }

  #loadTasksWithDelay(projectId: number, showOnlyMyTasks: boolean) {
    // Clear any existing timer
    if (this.#loadingTimer) {
      clearTimeout(this.#loadingTimer);
      this.#loadingTimer = null;
    }

    // Don't show loading state immediately
    this.isLoadingTasks.set(false);

    // Set a timer to show loading state if API takes longer than 150ms
    this.#loadingTimer = setTimeout(() => {
      this.isLoadingTasks.set(true);
      this.#loadingTimer = null;
    }, 150);

    // Make the API call
    this.#taskFacade.getTasks(projectId, showOnlyMyTasks);
  }

  onProjectSelected(projectId: number) {
    void this.#router.navigate(["/taken/project", projectId]);
    if (this.isMobile()) {
      this.mobileView.set('detail');
    }
  }

  onBackToProjectList() {
    this.mobileView.set('list');
    void this.#router.navigate(["/taken"]);
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

  onOpenDeleteProjectDialog() {
    const project = this.selectedProject();

    if (!project) {
      return;
    }

    const dialogRef = this.#dialog.open<boolean>(DeleteProjectDialogComponent, {
      data: { project } as DeleteProjectDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });

    dialogRef.closed.subscribe(result => {
      if (result) {
        void this.#router.navigate(["/taken"]);
      }
    });
  }

  onTaskFilterChanges(showOnlyMyTasks: boolean) {
    this.showOnlyMyTasks.set(showOnlyMyTasks);
    const selectedProjectId = this.selectedProjectId();

    if (selectedProjectId) {
      this.#loadTasksWithDelay(selectedProjectId, showOnlyMyTasks);
    }
  }

  onOpenCreateSectionDialog() {
    const selectedProject = this.selectedProject();

    if (!selectedProject) {
      return;
    }

    this.#dialog.open(SaveSectionDialogComponent, {
      data: { mode: "create", projectId: selectedProject.projectId } as SaveSectionDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  onOpenUpdateSectionDialog(sectionId: number) {
    const section = this.sections().find(s => s.sectionId === sectionId);
    const selectedProject = this.selectedProject();

    if (!section || !selectedProject) {
      return;
    }

    this.#dialog.open(SaveSectionDialogComponent, {
      data: { mode: "update", projectId: selectedProject.projectId, section } as SaveSectionDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  onOpenDeleteSectionDialog(sectionId: number) {
    const section = this.sections().find(s => s.sectionId === sectionId);
    const selectedProject = this.selectedProject();

    if (!section || !selectedProject) {
      return;
    }

    const dialogRef = this.#dialog.open<boolean>(DeleteSectionDialogComponent, {
      data: { section, projectId: selectedProject.projectId } as DeleteSectionDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });

    dialogRef.closed.subscribe(result => {
      if (result) {
        this.#taskFacade.getProjects();
      }
    });
  }

  onOpenCreateTaskDialog(sectionId: number) {
    const selectedProject = this.selectedProject();

    if (!selectedProject) {
      return;
    }

    this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "create", projectId: selectedProject.projectId, sectionId } as SaveTaskDialogData,
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
      data: { mode: "update", projectId: selectedProject.projectId, sectionId: task.sectionId, task } as SaveTaskDialogData,
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
          this.#taskFacade.getSections(projectId);
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
      [TaskStatus.Pending]: "Taak heropend",
      [TaskStatus.WaitingOn]: "Taak in wacht gezet"
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

  onSectionReorder(items: { sectionId: number; displayOrder: number }[]) {
    this.#taskFacade.reorderSections(items)
      .pipe(catchError((error: HttpErrorResponse) => {
        this.#notificationService.showError(extractErrorMessage(error));
        return EMPTY;
      }))
      .subscribe(() => this.#notificationService.showSuccess("Volgorde opgeslagen"));
  }

  onTaskReorder(items: { taskId: number; displayOrder: number }[]) {
    this.#taskFacade.reorderTasks(items)
      .pipe(catchError((error: HttpErrorResponse) => {
        this.#notificationService.showError(extractErrorMessage(error));
        return EMPTY;
      }))
      .subscribe(() => this.#notificationService.showSuccess("Volgorde opgeslagen"));
  }

  onSubtaskReorder(items: { taskId: number; displayOrder: number }[]) {
    this.#taskFacade.reorderTasks(items)
      .pipe(catchError((error: HttpErrorResponse) => {
        this.#notificationService.showError(extractErrorMessage(error));
        return EMPTY;
      }))
      .subscribe(() => this.#notificationService.showSuccess("Volgorde opgeslagen"));
  }

  onSubtaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    const statusMessages = {
      [TaskStatus.Ongoing]: "Subtaak gestart",
      [TaskStatus.Done]: "Subtaak voltooid",
      [TaskStatus.Pending]: "Subtaak heropend",
      [TaskStatus.WaitingOn]: "Subtaak in wacht gezet"
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
      sectionId: subtask.sectionId,
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
      waitingSince: subtask.waitingSince,
      subtaskCount: 0,
      completedSubtaskCount: 0,
      subtasks: [],
      displayOrder: subtask.displayOrder
    };

    this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "update-subtask", projectId: selectedProject.projectId, sectionId: subtask.sectionId, task: taskData } as SaveTaskDialogData,
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
      sectionId: subtask.sectionId,
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
      waitingSince: subtask.waitingSince,
      subtaskCount: 0,
      completedSubtaskCount: 0,
      subtasks: [],
      displayOrder: subtask.displayOrder
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
          this.#taskFacade.getSections(projectId);
          this.#taskFacade.getTasks(projectId, this.showOnlyMyTasks());
          this.#taskFacade.getProjects();
        }
      }
    });
  }
}
