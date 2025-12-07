import { inject, Injectable, signal } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { environment } from "../../../environments/environment";
import { Company } from "./models/company.enum";
import { finalize, Observable } from "rxjs";
import { Project } from "./models/project.model";
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
  #http = inject(HttpClient);
  #apiUrl = environment.apiUrl;

  #projects = signal<Project[]>([]);
  #isLoadingProjects = signal<boolean>(false);
  #companyFilter = signal<"ALL" | Company>("ALL");

  #tasks = signal<Task[]>([]);
  #isLoadingTasks = signal<boolean>(false);

  projects = this.#projects.asReadonly();
  isLoadingProjects = this.#isLoadingProjects.asReadonly();
  companyFilter = this.#companyFilter.asReadonly();

  tasks = this.#tasks.asReadonly();
  isLoadingTasks = this.#isLoadingTasks.asReadonly();

  getProjects(company?: Company): void {
    this.#isLoadingProjects.set(true);

    let params = new HttpParams();

    const filterToUse = company ?? (this.#companyFilter() === "ALL" ? undefined : this.#companyFilter());

    if (filterToUse) {
      params = params.set("company", filterToUse.toString());
    }

    this.#http
      .get<GetProjectsResponse>(`${this.#apiUrl}/api/projects`, { params })
      .pipe(finalize(() => this.#isLoadingProjects.set(false)))
      .subscribe(response => {
        this.#projects.set(response.projects);
      });
  }

  getTasks(projectId: number, showOnlyMyTasks = false): void {
    this.#isLoadingTasks.set(true);

    let params = new HttpParams()
      .set("projectId", projectId.toString())
      .set("onlyShowMyTasks", showOnlyMyTasks.toString());

    this.#http
      .get<GetTasksResponse>(`${this.#apiUrl}/api/tasks`, { params })
      .pipe(finalize(() => this.#isLoadingTasks.set(false)))
      .subscribe(response => {
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

  createProject(request: CreateProjectRequest): Observable<void> {
    return this.#http.post<void>(`${this.#apiUrl}/api/projects`, request);
  }

  updateProject(projectId: number, request: UpdateProjectRequest): Observable<void> {
    return this.#http.put<void>(`${this.#apiUrl}/api/projects/${projectId}`, request);
  }

  createTask(request: CreateTaskRequest): Observable<void> {
    return this.#http.post<void>(`${this.#apiUrl}/api/tasks`, request);
  }

  updateTask(taskId: number, request: UpdateTaskRequest): Observable<void> {
    return this.#http.put<void>(`${this.#apiUrl}/api/tasks/${taskId}`, request);
  }

  deleteTask(taskId: number): Observable<void> {
    return this.#http.delete<void>(`${this.#apiUrl}/api/tasks/${taskId}`);
  }

  updateTaskStatus(taskId: number, request: UpdateTaskStatusRequest): Observable<void> {
    return this.#http.patch<void>(`${this.#apiUrl}/api/tasks/${taskId}/status`, request);
  }
}
