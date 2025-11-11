import { Component, computed, inject, signal } from "@angular/core";
import { CommonModule } from "@angular/common";
import { ActivatedRoute, Router } from "@angular/router";
import { ProjectListComponent } from "../project-list/project-list.component";
import { ProjectDetailComponent } from "../project-detail/project-detail.component";
import { TaskFacade } from "../../task.facade";
import { Project } from "../../models/project.model";
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

@Component({
  selector: "app-tasks-page",
  standalone: true,
  imports: [CommonModule, ProjectListComponent, ProjectDetailComponent],
  templateUrl: "./tasks-page.html",
  styleUrl: "./tasks-page.scss"
})
export class TasksPage {
  // Inject dependencies
  facade = inject(TaskFacade);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  #dialog = inject(Dialog);

  // Expose signals from facade
  projects = this.facade.projects;
  tasks = this.facade.tasks;
  isLoadingProjects = this.facade.isLoadingProjects;
  isLoadingTasks = this.facade.isLoadingTasks;
  companyFilter = this.facade.companyFilter;

  // Local state
  selectedProjectId = signal<number | null>(null);
  showOnlyMyTasks = signal<boolean>(false);

  // Computed signals
  selectedProject = computed(() => {
    const id = this.selectedProjectId();
    return id ? this.projects().find(p => p.projectId === id) : undefined;
  });

  // Expose enum to template
  readonly Company = Company;

  // Track if we've done the initial load
  private hasInitiallyLoaded = false;

  constructor() {
    // Initial load of projects only if not already loaded (avoid flicker on first click)
    if (this.projects().length === 0 && !this.hasInitiallyLoaded) {
      this.hasInitiallyLoaded = true;
      this.facade.getProjects();
    }

    // Subscribe to route params and update selected project + load tasks
    this.route.paramMap.subscribe(params => {
      const projectId = params.get("id");
      if (projectId) {
        const idNum = Number(projectId);
        this.selectedProjectId.set(idNum);
        // Reset task visibility filter when switching projects
        this.showOnlyMyTasks.set(false);
        this.facade.getTasks(idNum, false);
      } else {
        this.selectedProjectId.set(null);
        this.facade.clearTasks();
      }
    });
  }

  onProjectSelected(projectId: number) {
    // Navigate to the project route
    console.log("Navigating to project:", projectId);
    this.router.navigate(["/taken/project", projectId]).then(
      success => console.log("Navigation success:", success),
      error => console.error("Navigation error:", error)
    );
  }

  onProjectCreate(project: Project) {
    this.facade.createProject({
      title: project.title,
      description: project.description,
      company: project.company
    });
  }

  onProjectUpdate(project: Project) {
    this.facade.updateProject(project.projectId, {
      title: project.title,
      description: project.description,
      company: project.company
    });
    // If updating currently selected project refresh selection reference
    if (this.selectedProjectId() === project.projectId) {
      // Trigger recompute by setting id again
      this.selectedProjectId.set(project.projectId);
    }
  }

  onNewProject() {
    const ref = this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: "create" } as SaveProjectDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
    ref.closed.subscribe(result => {
      if (result?.mode === "create") {
        this.facade.createProject({
          title: result.project.title,
          description: result.project.description,
          company: result.project.company
        });
      }
    });
  }

  onEditProjectRequested() {
    const proj = this.selectedProject();
    if (!proj) return;
    const ref = this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: "update", project: proj } as SaveProjectDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
    ref.closed.subscribe(result => {
      if (result?.mode === "update") {
        this.facade.updateProject(result.project.projectId, {
          title: result.project.title,
          description: result.project.description,
          company: result.project.company
        });
      }
    });
  }


  onTaskClicked(taskId: number) {
    const task = this.tasks().find(t => t.taskId === taskId);
    const proj = this.selectedProject();
    if (!task || !proj) return;
    const ref = this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "update", projectId: proj.projectId, task } as SaveTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
    ref.closed.subscribe(result => {
      if (result?.mode === "update") {
        const task = result.task;
        this.facade.updateTask(task.taskId, {
          title: task.title,
          description: task.description ?? undefined,
          typeId: task.typeId,
          dueDate: task.dueDate ?? "",
          assigneeId: task.employeeId ?? this.facade.employees().find(e => e.name === task.employeeName)?.id ?? 0,
          status: task.status,
          progress: task.subtaskCount > 0 ? Math.round((task.completedSubtaskCount / task.subtaskCount) * 100) : 0
        });
      }
    });
  }

  onTaskFilterToggled(showOnlyMine: boolean) {
    this.showOnlyMyTasks.set(showOnlyMine);
    const pid = this.selectedProjectId();
    if (pid) {
      this.facade.getTasks(pid, showOnlyMine);
    }
  }

  onTaskToggled(taskId: number) {
    console.log("Task toggled:", taskId);
    const task = this.tasks().find(t => t.taskId === taskId);
    if (!task) return;
    // Toggle task status (simple implementation - could be more sophisticated)
    const newStatus = task.completedAt ? 0 : 2; // 0 = Pending, 2 = Done
    this.facade.updateTaskStatus(taskId, { status: newStatus });
  }

  filterCompany(filter: "ALL" | Company) {
    // Clear current project selection and route when changing company filter
    this.selectedProjectId.set(null);
    this.router.navigate(["/taken"]).catch(() => {
    });
    this.facade.setCompanyFilter(filter);
  }

  onCreateTaskRequested() {
    const proj = this.selectedProject();
    if (!proj) return;
    const ref = this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: "create", projectId: proj.projectId } as SaveTaskDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
    ref.closed.subscribe(result => {
      if (result?.mode === "create") {
        const task = result.task;
        this.facade.createTask({
          projectId: task.projectId,
          title: task.title,
          taskTypeId: task.typeId,
          dueDate: task.dueDate ?? "",
          employeeId: task.employeeId ?? this.facade.employees().find(e => e.name === task.employeeName)?.id ?? 0,
          description: task.description ?? undefined
        });
      }
    });
  }
}
