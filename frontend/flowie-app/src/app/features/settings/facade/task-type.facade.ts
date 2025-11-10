import { Injectable, signal, inject } from "@angular/core";
import { TaskTypeApiService, TaskTypeDto } from "../../../core/services/task-type-api.service";

@Injectable({ providedIn: "root" })
export class TaskTypeFacade {
  private taskTypeApi = inject(TaskTypeApiService);

  #taskTypes = signal<TaskTypeDto[]>([]);
  #isLoading = signal<boolean>(false);

  taskTypes = this.#taskTypes.asReadonly();
  isLoading = this.#isLoading.asReadonly();

  getTaskTypes(): void {
    this.#isLoading.set(true);

    this.taskTypeApi.getTaskTypes().subscribe({
      next: (response) => {
        this.#taskTypes.set(response.taskTypes);
        this.#isLoading.set(false);
      },
      error: (error) => {
        console.error("Error loading task types:", error);
        this.#taskTypes.set([]);
        this.#isLoading.set(false);
      },
    });
  }

  add(name: string): void {
    const trimmed = name.trim();
    if (!trimmed) return;
    const exists = this.#taskTypes().some(
      (t) => t.name.toLowerCase() === trimmed.toLowerCase(),
    );
    if (exists) return;

    this.taskTypeApi.createTaskType({ name: trimmed }).subscribe({
      next: () => {
        // Refresh task types list after creation
        this.getTaskTypes();
      },
      error: (error) => {
        console.error("Error creating task type:", error);
      },
    });
  }

  remove(id: number): void {
    this.taskTypeApi.deleteTaskType(id).subscribe({
      next: () => {
        // Update local state after deletion
        this.#taskTypes.update((list) => list.filter((t) => t.id !== id));
      },
      error: (error) => {
        console.error("Error deleting task type:", error);
      },
    });
  }
}
