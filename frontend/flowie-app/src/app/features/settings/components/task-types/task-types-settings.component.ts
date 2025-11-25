import { Component, computed, inject, signal } from '@angular/core';
import { NgFor, NgIf } from '@angular/common';
import { Dialog } from '@angular/cdk/dialog';
import { TaskTypeFacade, TaskType } from '../../facade/task-type.facade';
import { ConfirmDeleteDialogComponent, ConfirmDeleteDialogData, ConfirmDeleteDialogResult } from '../confirm-delete-dialog/confirm-delete-dialog.component';

@Component({
  selector: 'app-task-types-settings',
  standalone: true,
  imports: [NgFor, NgIf],
  templateUrl: './task-types-settings.component.html',
  styleUrl: './task-types-settings.component.scss'
})
export class TaskTypesSettingsComponent {
  #service = inject(TaskTypeFacade);
  #dialog = inject(Dialog);

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

  remove(taskType: TaskType): void {
    this.#dialog.open<ConfirmDeleteDialogResult>(ConfirmDeleteDialogComponent, {
      data: { taskType } as ConfirmDeleteDialogData,
      backdropClass: ['fixed', 'inset-0', 'bg-black/40'],
      panelClass: ['dialog-panel', 'flex', 'items-center', 'justify-center']
    });
  }
}
