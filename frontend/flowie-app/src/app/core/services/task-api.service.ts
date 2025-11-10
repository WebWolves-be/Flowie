import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../../environments/environment";
import { TaskStatus } from "../../features/tasks/models/task-status.enum";

export interface SubtaskDto {
  title: string;
  assignee: { name: string };
  dueDate: string;
  done: boolean;
  status: TaskStatus;
  statusName: string;
}

export interface TaskDto {
  id: number;
  projectId: number;
  title: string;
  description?: string;
  typeId: number;
  typeName: string;
  status: TaskStatus;
  statusName: string;
  dueDate: string;
  progress: number;
  assignee: { name: string };
  createdAt: string;
  updatedAt?: string;
  completedAt?: string | null;
  subtasks?: SubtaskDto[];
  subtaskCount: number;
  completedSubtaskCount: number;
}

export interface GetTasksResponse {
  tasks: TaskDto[];
}

export interface CreateTaskRequest {
  projectId: number;
  title: string;
  taskTypeId: number;
  dueDate: string;
  employeeId: number;
  description?: string;
  parentTaskId?: number;
}

export interface UpdateTaskRequest {
  taskId: number;
  title: string;
  description?: string;
  typeId: number;
  dueDate: string;
  assigneeId: number;
  status: TaskStatus;
  progress: number;
}

export interface UpdateTaskStatusRequest {
  status: TaskStatus;
}

@Injectable({
  providedIn: "root",
})
export class TaskApiService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/tasks`;

  getTasks(
    projectId: number,
    onlyShowMyTasks: boolean = false,
  ): Observable<GetTasksResponse> {
    let params = new HttpParams()
      .set("projectId", projectId.toString())
      .set("onlyShowMyTasks", onlyShowMyTasks.toString());

    return this.http.get<GetTasksResponse>(this.apiUrl, { params });
  }

  getTaskById(id: number): Observable<TaskDto> {
    return this.http.get<TaskDto>(`${this.apiUrl}/${id}`);
  }

  createTask(request: CreateTaskRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, request);
  }

  updateTask(request: UpdateTaskRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl, request);
  }

  updateTaskStatus(
    taskId: number,
    request: UpdateTaskStatusRequest,
  ): Observable<void> {
    return this.http.patch<void>(`${this.apiUrl}/${taskId}/status`, request);
  }

  deleteTask(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
