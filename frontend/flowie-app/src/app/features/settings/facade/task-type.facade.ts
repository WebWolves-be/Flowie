import { Injectable, signal, inject, effect } from "@angular/core";
import { TaskFacade } from "../../tasks/task.facade";

export interface TaskType {
  id: number;
  name: string;
}

@Injectable({ providedIn: "root" })
export class TaskTypeFacade {
  private taskFacade = inject(TaskFacade);

  #taskTypes = signal<TaskType[]>([]);
  #isLoading = signal<boolean>(false);

  taskTypes = this.#taskTypes.asReadonly();
  isLoading = this.#isLoading.asReadonly();

  constructor() {
    // Sync with TaskFacade state
    effect(() => {
      const taskTypes = this.taskFacade.taskTypes();
      this.#taskTypes.set(taskTypes.map((dto) => ({
        id: dto.id,
        name: dto.name,
      })));
    });

    effect(() => {
      this.#isLoading.set(this.taskFacade.isLoadingTaskTypes());
    });
  }

  getTaskTypes(): void {
    this.taskFacade.getTaskTypes();
  }

  add(name: string): void {
    const trimmed = name.trim();
    if (!trimmed) return;
    const exists = this.#taskTypes().some(
      (t) => t.name.toLowerCase() === trimmed.toLowerCase(),
    );
    if (exists) return;

    this.taskFacade.createTaskType({ name: trimmed });
  }

  remove(id: number) {
    return this.taskFacade.deleteTaskType(id);
  }
}
