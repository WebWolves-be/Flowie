import { inject, Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import { finalize, Observable } from "rxjs";
import { TaskType } from "../models/task-type.model";
import { GetTaskTypesResponse } from "../models/get-task-types-response.model";
import { CreateTaskTypeRequest } from "../models/create-task-type-request.model";

@Injectable({ providedIn: "root" })
export class TaskTypeFacade {
  readonly #http = inject(HttpClient);
  readonly #apiUrl = environment.apiUrl;

  #taskTypes = signal<TaskType[]>([]);
  #isLoadingTaskTypes = signal<boolean>(false);

  taskTypes = this.#taskTypes.asReadonly();
  isLoadingTaskTypes = this.#isLoadingTaskTypes.asReadonly();

  getTaskTypes(): void {
    this.#isLoadingTaskTypes.set(true);

    this.#http
      .get<GetTaskTypesResponse>(`${this.#apiUrl}/api/task-types`)
      .pipe(finalize(() => this.#isLoadingTaskTypes.set(false)))
      .subscribe(response => {
        this.#taskTypes.set(response.taskTypes);
      });
  }

  createTaskType(request: CreateTaskTypeRequest): Observable<void> {
    return this.#http.post<void>(`${this.#apiUrl}/api/task-types`, request);
  }

  deleteTaskType(id: number): Observable<void> {
    return this.#http.delete<void>(`${this.#apiUrl}/api/task-types/${id}`);
  }
}
