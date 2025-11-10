import { Injectable, signal, computed, inject } from "@angular/core";
import { Company } from "../models/company.enum";
import { TaskStatus } from "../models/task-status.enum";
import { ProjectApiService, ProjectDto } from "../../../core/services/project-api.service";
import { TaskApiService, TaskDto } from "../../../core/services/task-api.service";
import { EmployeeApiService, EmployeeDto } from "../../../core/services/employee-api.service";

@Injectable({
  providedIn: "root",
})
export class TaskFacade {
  private projectApi = inject(ProjectApiService);
  private taskApi = inject(TaskApiService);
  private employeeApi = inject(EmployeeApiService);

  #projects = signal<ProjectDto[]>([]);
  #tasks = signal<TaskDto[]>([]);
  #employees = signal<EmployeeDto[]>([]);
  #isLoadingProjects = signal<boolean>(false);
  #isLoadingTasks = signal<boolean>(false);
  #isLoadingEmployees = signal<boolean>(false);
  #companyFilter = signal<"ALL" | Company>("ALL");

  projects = this.#projects.asReadonly();
  tasks = this.#tasks.asReadonly();
  employees = this.#employees.asReadonly();
  isLoadingProjects = this.#isLoadingProjects.asReadonly();
  isLoadingTasks = this.#isLoadingTasks.asReadonly();
  isLoadingEmployees = this.#isLoadingEmployees.asReadonly();
  companyFilter = this.#companyFilter.asReadonly();

  getProjects(company?: Company): void {
    this.#isLoadingProjects.set(true);

    this.projectApi.getProjects(company).subscribe({
      next: (response) => {
        // Calculate progress for each project
        const projects = response.projects.map((dto) => ({
          ...dto,
          progress:
            dto.taskCount > 0
              ? Math.round((dto.completedTaskCount / dto.taskCount) * 100)
              : 0,
        }));

        this.#projects.set(projects);
        this.#isLoadingProjects.set(false);
      },
      error: (error) => {
        console.error("Error loading projects:", error);
        this.#projects.set([]);
        this.#isLoadingProjects.set(false);
      },
    });
  }

  getTasks(projectId: number, showOnlyMyTasks = false): void {
    this.#isLoadingTasks.set(true);

    this.taskApi.getTasks(projectId, showOnlyMyTasks).subscribe({
      next: (response) => {
        this.#tasks.set(response.tasks);
        this.#isLoadingTasks.set(false);
      },
      error: (error) => {
        console.error("Error loading tasks:", error);
        this.#tasks.set([]);
        this.#isLoadingTasks.set(false);
      },
    });
  }

  clearTasks(): void {
    this.#tasks.set([]);
  }

  setCompanyFilter(filter: "ALL" | Company): void {
    this.#companyFilter.set(filter);
    this.getProjects(filter === "ALL" ? undefined : filter);
  }

  createProject(project: Omit<ProjectDto, "id">): void {
    // Call API to create project
    this.projectApi
      .createProject({
        title: project.title,
        description: project.description || undefined,
        company: project.company,
      })
      .subscribe({
        next: () => {
          // Refresh project list after creation
          const filter = this.#companyFilter();
          this.getProjects(filter === "ALL" ? undefined : filter);
        },
        error: (error) => {
          console.error("Error creating project:", error);
        },
      });
  }

  updateProject(updated: ProjectDto): void {
    // Call API to update project
    this.projectApi
      .updateProject({
        id: updated.id,
        title: updated.title,
        description: updated.description || undefined,
        company: updated.company,
      })
      .subscribe({
        next: () => {
          // Update local state
          const current = this.#projects();
          const next = current.map((p) =>
            p.id === updated.id ? { ...p, ...updated } : p,
          );
          this.#projects.set(next);
        },
        error: (error) => {
          console.error("Error updating project:", error);
        },
      });
  }

  createTask(task: Omit<TaskDto, "id">): void {
    // Call API to create task
    if (!task.typeId || !task.dueDate || !task.assignee.id) {
      console.error("Missing required task fields");
      return;
    }

    this.taskApi
      .createTask({
        projectId: task.projectId,
        title: task.title,
        taskTypeId: task.typeId,
        dueDate: task.dueDate,
        employeeId: task.assignee.id,
        description: task.description || undefined,
        parentTaskId: task.parentTaskId || undefined,
      })
      .subscribe({
        next: () => {
          // Refresh task list after creation
          this.getTasks(task.projectId, false);
        },
        error: (error) => {
          console.error("Error creating task:", error);
        },
      });
  }

  updateTask(updated: TaskDto): void {
    // Call API to update task
    if (
      !updated.typeId ||
      !updated.dueDate ||
      !updated.assignee.id ||
      !updated.status
    ) {
      console.error("Missing required task fields for update");
      return;
    }

    this.taskApi
      .updateTask({
        id: updated.id,
        title: updated.title,
        description: updated.description || undefined,
        typeId: updated.typeId,
        dueDate: updated.dueDate,
        assigneeId: updated.assignee.id,
        status: updated.status,
        progress: updated.progress || 0,
      })
      .subscribe({
        next: () => {
          // Update local state
          const current = this.#tasks();
          const next = current.map((t) =>
            t.id === updated.id ? { ...t, ...updated } : t,
          );
          this.#tasks.set(next);
        },
        error: (error) => {
          console.error("Error updating task:", error);
        },
      });
  }

  getEmployees(): void {
    this.#isLoadingEmployees.set(true);

    // Note: Backend doesn't have employee endpoint yet, so this will fail
    // Consider keeping mock data or implementing backend endpoint
    this.employeeApi.getEmployees().subscribe({
      next: (response) => {
        this.#employees.set(response.employees);
        this.#isLoadingEmployees.set(false);
      },
      error: (error) => {
        console.error(
          "Error loading employees (endpoint may not exist):",
          error,
        );
        // Fallback to mock data for now
        const mockEmployees: EmployeeDto[] = [
          { id: 1, name: "Amalia Van Dosselaer" },
          { id: 2, name: "Peter Carrein" },
          { id: 3, name: "Jens Declerck" },
          { id: 4, name: "Sophie Vermeulen" },
          { id: 5, name: "Tom Janssens" },
          { id: 6, name: "Lisa Peeters" },
          { id: 7, name: "Marc De Vos" },
          { id: 8, name: "Emma Claes" },
        ];
        this.#employees.set(mockEmployees);
        this.#isLoadingEmployees.set(false);
      },
    });
  }
}
