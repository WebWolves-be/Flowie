import { Component, computed, DestroyRef, inject, signal, OnInit } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import { ProjectListComponent } from "../project-list/project-list.component";
import { ProjectDetailComponent } from "../project-detail/project-detail.component";
import { TaskFacade } from "../../task.facade";
import { Company } from "../../models/company.enum";
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
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";

@Component({
  selector: "app-tasks-page",
  standalone: true,
  imports: [CommonModule, ProjectListComponent, ProjectDetailComponent],
  templateUrl: "./tasks-page.html",
  styleUrl: "./tasks-page.scss"
})
export class TasksPage implements OnInit {
  #facade = inject(TaskFacade);
  #router = inject(Router);
  #route = inject(ActivatedRoute);
  #dialog = inject(Dialog);
  #destroy = inject(DestroyRef);

  #hasInitiallyLoaded = false;

  readonly Company = Company;

  projects = this.#facade.projects;
  isLoadingProjects = this.#facade.isLoadingProjects;

  tasks = this.#facade.tasks;
  isLoadingTasks = this.#facade.isLoadingTasks;

  companyFilter = this.#facade.companyFilter;

  selectedProjectId = signal<number | null>(null);
  showOnlyMyTasks = signal<boolean>(false);

  selectedProject = computed(() => {
    const id = this.selectedProjectId();
    return id ? this.projects().find(p => p.projectId === id) : undefined;
  });

  ngOnInit(): void {
    if (this.projects().length === 0 && !this.#hasInitiallyLoaded) {
      this.#hasInitiallyLoaded = true;
      this.#facade.getProjects();
    }

    this.#route.paramMap.pipe(takeUntilDestroyed(this.#destroy)).subscribe(params => {
      const projectId = params.get("id");
      if (projectId) {
        const idNum = Number(projectId);

        this.selectedProjectId.set(idNum);
        this.showOnlyMyTasks.set(false);

        this.#facade.getTasks(idNum, false);
      } else {
        this.#facade.clearTasks();

        this.selectedProjectId.set(null);
      }
    });
  }

  onProjectSelected(projectId: number) {
    void this.#router.navigate(["/taken/project", projectId]);
  }

  onOpenCreateProjectDialog() {
    const ref = this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: "create" } as SaveProjectDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });

    // Dialog handles save via facade
  }

  onOpenUpdateProjectDialog() {
    const proj = this.selectedProject();

    if (!proj) {
      return;
    }

    const ref = this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: "update", project: proj } as SaveProjectDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });

    // Dialog handles save via facade
  }

  onOpenCreateTaskDialog() {
    const selectedProject = this.selectedProject();

    if (!selectedProject) {
      return;
    }

    const ref = this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "create", projectId: selectedProject.projectId } as SaveTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });

    ref.closed
      .pipe(takeUntilDestroyed(this.#destroy))
      .subscribe(result => {
        if (result?.mode === "create") {
          const task = result.task;
          this.#facade.createTask({
            projectId: task.projectId,
            title: task.title,
            taskTypeId: task.typeId,
            dueDate: task.dueDate ?? "",
            employeeId: task.employeeId ?? this.#facade.employees().find(e => e.name === task.employeeName)?.id ?? 0,
            description: task.description ?? undefined
          });
        }
      });
  }

  onOpenUpdateTaskDialog(taskId: number) {
    const task = this.tasks().find(t => t.taskId === taskId);
    const selectedProject = this.selectedProject();

    if (!task || !selectedProject) {
      return;
    }

    const ref = this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "update", projectId: selectedProject.projectId, task } as SaveTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });

    ref.closed
      .pipe(takeUntilDestroyed(this.#destroy))
      .subscribe(result => {
        if (result?.mode === "update") {
          const task = result.task;
          this.#facade.updateTask(task.taskId, {
            title: task.title,
            description: task.description ?? undefined,
            typeId: task.typeId,
            dueDate: task.dueDate ?? "",
            assigneeId: task.employeeId ?? this.#facade.employees().find(e => e.name === task.employeeName)?.id ?? 0,
            status: task.status,
            progress: task.subtaskCount > 0 ? Math.round((task.completedSubtaskCount / task.subtaskCount) * 100) : 0
          });
        }
      });
  }

  onTaskFilterChanges(showOnlyMyTasks: boolean) {
    this.showOnlyMyTasks.set(showOnlyMyTasks);
    const selectedProjectId = this.selectedProjectId();

    if (selectedProjectId) {
      this.#facade.getTasks(selectedProjectId, showOnlyMyTasks);
    }
  }

  onTaskStatusChanges(taskId: number) {
    const task = this.tasks().find(t => t.taskId === taskId);

    if (!task) {
      return;
    }

    const newStatus = task.completedAt ? 0 : 2;

    this.#facade.updateTaskStatus(taskId, { status: newStatus });
  }

  filterProjectsByCompany(filter: "ALL" | Company) {
    this.selectedProjectId.set(null);
    this.#facade.setCompanyFilter(filter);

    void this.#router.navigate(["/taken"]);
  }
}
