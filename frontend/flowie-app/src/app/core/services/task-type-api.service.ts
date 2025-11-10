import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

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
  providedIn: 'root'
})
export class TaskTypeApiService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/task-types`;

  getTaskTypes(): Observable<GetTaskTypesResponse> {
    return this.http.get<GetTaskTypesResponse>(this.apiUrl);
  }

  createTaskType(request: CreateTaskTypeRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, request);
  }

  deleteTaskType(id: number): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/${id}`);
  }
}
