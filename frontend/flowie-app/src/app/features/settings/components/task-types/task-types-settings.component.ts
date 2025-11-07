import { Component, computed, inject, signal } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { TaskTypeFacade } from '../../facade/task-type.facade';

@Component({
  selector: 'app-task-types-settings',
  standalone: true,
  imports: [NgFor, NgIf],
  templateUrl: './task-types-settings.component.html',
  styleUrl: './task-types-settings.component.scss'
})
export class TaskTypesSettingsComponent {
  #service = inject(TaskTypeFacade);
  taskTypes = this.#service.taskTypes;
  isLoading = this.#service.isLoading;

  newName = signal('');
  canAdd = computed(() => this.newName().trim().length > 0);

  constructor() {
    this.#service.getTaskTypes();
  }

  add(): void {
    this.#service.add(this.newName());
    this.newName.set('');
  }

  remove(id: number): void {
    this.#service.remove(id);
  }
}
