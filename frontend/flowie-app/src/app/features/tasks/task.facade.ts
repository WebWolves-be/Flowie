import { inject, Injectable, signal } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../environments/environment";
import { Company } from "./models/company.enum";
import { catchError, EMPTY, finalize } from "rxjs";
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

  projects = this.#projects.asReadonly();
  isLoadingProjects = this.#isLoadingProjects.asReadonly();
  isSavingProject = this.#isSavingProject.asReadonly();

  companyFilter = this.#companyFilter.asReadonly();

  tasks = this.#tasks.asReadonly();
  isLoadingTasks = this.#isLoadingTasks.asReadonly();
  isSavingTask = this.#isSavingTask.asReadonly();

  employees = this.#employees.asReadonly();

  getProjects(company?: Company): void {
    this.#isLoadingProjects.set(true);

    let params = new HttpParams();

    if (company) {
      params = params.set("company", company.toString());
    }

    this.http
      .get<GetProjectsResponse>(`${this.apiUrl}/api/projects`, { params })
      .pipe(
        catchError((error) => {
          return EMPTY;
        }),
        finalize(() => this.#isLoadingProjects.set(false))
      )
      .subscribe((response) => {
        this.#projects.set(response.projects);
      });
  }

  getTasks(projectId: number, showOnlyMyTasks = false): void {
    this.#isLoadingTasks.set(true);

    let params = new HttpParams()
      .set("projectId", projectId.toString())
      .set("onlyShowMyTasks", showOnlyMyTasks.toString());

    this.http
      .get<GetTasksResponse>(`${this.apiUrl}/api/tasks`, { params })
      .pipe(
        catchError((error) => {
          return EMPTY;
        }),
        finalize(() => this.#isLoadingTasks.set(false))
      )
      .subscribe((response) => {
        this.#tasks.set(response.tasks);
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
      .pipe(
        catchError((error) => {
          console.error("Error creating project:", error);
          return EMPTY;
        }),
        finalize(() => this.#isSavingProject.set(false))
      )
      .subscribe(() => {
        const filter = this.#companyFilter();
        this.getProjects(filter === "ALL" ? undefined : filter);
      });
  }

  updateProject(projectId: number, request: UpdateProjectRequest): void {
    this.#isSavingProject.set(true);

    this.http
      .put<void>(`${this.apiUrl}/api/projects/${projectId}`, request)
      .pipe(
        catchError((error) => {
          console.error("Error updating project:", error);
          return EMPTY;
        }),
        finalize(() => this.#isSavingProject.set(false))
      )
      .subscribe(() => {
        const filter = this.#companyFilter();
        this.getProjects(filter === "ALL" ? undefined : filter);
      });
  }

  createTask(request: CreateTaskRequest): void {
    this.#isSavingTask.set(true);

    this.http
      .post<void>(`${this.apiUrl}/api/tasks`, request)
      .pipe(
        catchError((error) => {
          console.error("Error creating task:", error);
          return EMPTY;
        }),
        finalize(() => this.#isSavingTask.set(false))
      )
      .subscribe(() => {
        this.getTasks(request.projectId, false);
      });
  }

  updateTask(taskId: number, request: UpdateTaskRequest): void {
    this.#isSavingTask.set(true);

    this.http
      .put<void>(`${this.apiUrl}/api/tasks/${taskId}`, request)
      .pipe(
        catchError((error) => {
          console.error("Error updating task:", error);
          return EMPTY;
        }),
        finalize(() => this.#isSavingTask.set(false))
      )
      .subscribe(() => {
        const tasks = this.#tasks();
        if (tasks.length > 0) {
          this.getTasks(tasks[0].projectId, false);
        }
      });
  }

  updateTaskStatus(taskId: number, request: UpdateTaskStatusRequest): void {
    this.#isSavingTask.set(true);

    this.http
      .patch<void>(`${this.apiUrl}/api/tasks/${taskId}/status`, request)
      .pipe(
        catchError(() => {
          return EMPTY;
        }),
        finalize(() => this.#isSavingTask.set(false))
      )
      .subscribe(() => {
        const tasks = this.#tasks();
        if (tasks.length > 0) {
          this.getTasks(tasks[0].projectId, false);
        }
      });
  }

  deleteTask(taskId: number, projectId: number): void {
    this.#isSavingTask.set(true);

    this.http
      .delete<void>(`${this.apiUrl}/api/tasks/${taskId}`)
      .pipe(
        catchError((error) => {
          return EMPTY;
        }),
        finalize(() => this.#isSavingTask.set(false))
      )
      .subscribe(() => {
        this.getTasks(projectId, false);
      });
  }

  getEmployees(): void {
    this.#isLoadingEmployees.set(true);

    this.http
      .get<{ employees: Employee[] }>(`${this.apiUrl}/api/employees`)
      .pipe(
        catchError((error) => {
          console.error("Error loading employees:", error);
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
          return EMPTY;
        }),
        finalize(() => this.#isLoadingEmployees.set(false))
      )
      .subscribe((response) => {
        this.#employees.set(response.employees);
      });
  }
}
