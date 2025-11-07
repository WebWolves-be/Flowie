import { Injectable, signal } from '@angular/core';

export interface TaskType {
  id: number;
  name: string;
}

@Injectable({ providedIn: 'root' })
export class TaskTypeFacade {
  #taskTypes = signal<TaskType[]>([]);
  #isLoading = signal<boolean>(false);

  taskTypes = this.#taskTypes.asReadonly();
  isLoading = this.#isLoading.asReadonly();

  getTaskTypes(): void {
    this.#isLoading.set(true);
    
    // Simulate API delay
    setTimeout(() => {
      const mockTaskTypes: TaskType[] = [
        { id: 1, name: 'Compromis' },
        { id: 2, name: 'Ontwerp' },
        { id: 3, name: 'Communicatie' },
        { id: 4, name: 'Financiën' },
        { id: 5, name: 'Marketing' },
        { id: 6, name: 'Documentatie' },
        { id: 7, name: 'Juridisch' },
        { id: 8, name: 'Kwaliteit' },
        { id: 9, name: 'Planning' },
        { id: 10, name: 'Analyse' },
        { id: 11, name: 'Risico' }
      ];
      
      this.#taskTypes.set(mockTaskTypes);
      this.#isLoading.set(false);
    }, 300);
  }

  add(name: string): void {
    const trimmed = name.trim();
    if (!trimmed) return;
    const exists = this.#taskTypes().some(t => t.name.toLowerCase() === trimmed.toLowerCase());
    if (exists) return;
    
    // Simulate API call
    const newType: TaskType = { id: Date.now(), name: trimmed };
    this.#taskTypes.update(list => [...list, newType]);
  }

  remove(id: number): void {
    // Simulate API call
    this.#taskTypes.update(list => list.filter(t => t.id !== id));
  }
}