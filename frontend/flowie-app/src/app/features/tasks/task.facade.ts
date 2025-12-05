import { inject, Injectable, signal } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../environments/environment";
import { Company } from "./models/company.enum";
import { finalize } from "rxjs";
import { Project } from "./models/project.model";
import { Employee } from "./models/employee.model";
import { Task } from "./models/task.model";
import { GetProjectsResponse } from "./models/get-projects-response.model";
import { GetTasksResponse } from "./models/get-tasks-response.model";
import { CreateProjectRequest } from "./models/create-project-request.model";
import { UpdateProjectRequest } from "./models/update-project-request.model";
import { CreateTaskRequest } from "./models/create-task-request.model";
import { UpdateTaskRequest } from "./models/update-task-request.model";
import { UpdateTaskStatusRequest } from "./models/update-task-status-request.model";

export interface TaskTypeDto {
  id: number;
  name: string;
}

export interface GetTaskTypesResponse {
  taskTypes: TaskTypeDto[];
}

export interface CreateTaskTypeRequest {
  name: string;
}

@Injectable({
  providedIn: "root"
})
export class TaskFacade {
  private http = inject(HttpClient);
  private apiUrl = environment.apiUrl;

  #projects = signal<Project[]>([]);
  #isLoadingProjects = signal<boolean>(false);
  #isSavingProject = signal<boolean>(false);

  #tasks = signal<Task[]>([]);
  #isLoadingTasks = signal<boolean>(false);
  #isSavingTask = signal<boolean>(false);

  #companyFilter = signal<"ALL" | Company>("ALL");

  #employees = signal<Employee[]>([]);
  #isLoadingEmployees = signal<boolean>(false);

  #taskTypes = signal<TaskTypeDto[]>([]);
  #isLoadingTaskTypes = signal<boolean>(false);

  projects = this.#projects.asReadonly();
  isLoadingProjects = this.#isLoadingProjects.asReadonly();
  isSavingProject = this.#isSavingProject.asReadonly();

  companyFilter = this.#companyFilter.asReadonly();

  tasks = this.#tasks.asReadonly();
  isLoadingTasks = this.#isLoadingTasks.asReadonly();
  isSavingTask = this.#isSavingTask.asReadonly();

  employees = this.#employees.asReadonly();

  taskTypes = this.#taskTypes.asReadonly();
  isLoadingTaskTypes = this.#isLoadingTaskTypes.asReadonly();

  getProjects(company?: Company): void {
    this.#isLoadingProjects.set(true);

    let params = new HttpParams();

    if (company) {
      params = params.set("company", company.toString());
    }

    this.http
      .get<GetProjectsResponse>(`${this.apiUrl}/api/projects`, { params })
      .pipe(finalize(() => this.#isLoadingProjects.set(false)))
      .subscribe({
        next: (response) => {
          this.#projects.set(response.projects);
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  getTasks(projectId: number, showOnlyMyTasks = false): void {
    this.#isLoadingTasks.set(true);

    let params = new HttpParams()
      .set("projectId", projectId.toString())
      .set("onlyShowMyTasks", showOnlyMyTasks.toString());

    this.http
      .get<GetTasksResponse>(`${this.apiUrl}/api/tasks`, { params })
      .pipe(finalize(() => this.#isLoadingTasks.set(false)))
      .subscribe({
        next: (response) => {
          this.#tasks.set(response.tasks);
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  clearTasks(): void {
    this.#tasks.set([]);
  }

  setCompanyFilter(filter: "ALL" | Company): void {
    this.#companyFilter.set(filter);
    this.getProjects(filter === "ALL" ? undefined : filter);
  }

  createProject(request: CreateProjectRequest): void {
    this.#isSavingProject.set(true);

    this.http
      .post<void>(`${this.apiUrl}/api/projects`, request)
      .pipe(finalize(() => this.#isSavingProject.set(false)))
      .subscribe({
        next: () => {
          const filter = this.#companyFilter();
          this.getProjects(filter === "ALL" ? undefined : filter);
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  updateProject(projectId: number, request: UpdateProjectRequest): void {
    this.#isSavingProject.set(true);

    this.http
      .put<void>(`${this.apiUrl}/api/projects/${projectId}`, request)
      .pipe(finalize(() => this.#isSavingProject.set(false)))
      .subscribe({
        next: () => {
          const filter = this.#companyFilter();
          this.getProjects(filter === "ALL" ? undefined : filter);
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  createTask(request: CreateTaskRequest): void {
    this.#isSavingTask.set(true);

    this.http
      .post<void>(`${this.apiUrl}/api/tasks`, request)
      .pipe(finalize(() => this.#isSavingTask.set(false)))
      .subscribe({
        next: () => {
          this.getTasks(request.projectId, false);
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  updateTask(taskId: number, request: UpdateTaskRequest): void {
    this.#isSavingTask.set(true);

    this.http
      .put<void>(`${this.apiUrl}/api/tasks/${taskId}`, request)
      .pipe(finalize(() => this.#isSavingTask.set(false)))
      .subscribe({
        next: () => {
          const tasks = this.#tasks();
          if (tasks.length > 0) {
            this.getTasks(tasks[0].projectId, false);
          }
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  updateTaskStatus(taskId: number, request: UpdateTaskStatusRequest): void {
    this.#isSavingTask.set(true);

    this.http
      .patch<void>(`${this.apiUrl}/api/tasks/${taskId}/status`, request)
      .pipe(finalize(() => this.#isSavingTask.set(false)))
      .subscribe({
        next: () => {
          const tasks = this.#tasks();
          if (tasks.length > 0) {
            this.getTasks(tasks[0].projectId, false);
          }
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  deleteTask(taskId: number, projectId: number): void {
    this.#isSavingTask.set(true);

    this.http
      .delete<void>(`${this.apiUrl}/api/tasks/${taskId}`)
      .pipe(finalize(() => this.#isSavingTask.set(false)))
      .subscribe({
        next: () => {
          this.getTasks(projectId, false);
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  getEmployees(): void {
    this.#isLoadingEmployees.set(true);

    this.http
      .get<{ employees: Employee[] }>(`${this.apiUrl}/api/employees`)
      .pipe(finalize(() => this.#isLoadingEmployees.set(false)))
      .subscribe({
        next: (response) => {
          this.#employees.set(response.employees);
        },
        error: () => {
          // Fallback to mock data for employees
          const mockEmployees: Employee[] = [
            { id: 1, name: "Amalia Van Dosselaer" },
            { id: 2, name: "Peter Carrein" },
            { id: 3, name: "Jens Declerck" },
            { id: 4, name: "Sophie Vermeulen" },
            { id: 5, name: "Tom Janssens" },
            { id: 6, name: "Lisa Peeters" },
            { id: 7, name: "Marc De Vos" },
            { id: 8, name: "Emma Claes" }
          ];
          this.#employees.set(mockEmployees);
        }
      });
  }

  getTaskTypes(): void {
    this.#isLoadingTaskTypes.set(true);

    this.http
      .get<GetTaskTypesResponse>(`${this.apiUrl}/api/task-types`)
      .pipe(finalize(() => this.#isLoadingTaskTypes.set(false)))
      .subscribe({
        next: (response) => {
          this.#taskTypes.set(response.taskTypes);
        },
        error: () => {
          this.#taskTypes.set([]);
        }
      });
  }

  createTaskType(request: CreateTaskTypeRequest): void {
    this.http
      .post<void>(`${this.apiUrl}/api/task-types`, request)
      .subscribe({
        next: () => {
          this.getTaskTypes();
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }

  deleteTaskType(id: number): void {
    this.http
      .delete<void>(`${this.apiUrl}/api/task-types/${id}`)
      .subscribe({
        next: () => {
          this.#taskTypes.update((list) => list.filter((t) => t.id !== id));
        },
        error: () => {
          // Error handled by interceptor
        }
      });
  }
}
