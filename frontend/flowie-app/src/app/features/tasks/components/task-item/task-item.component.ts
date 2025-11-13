import { Component, computed, input, output, signal } from "@angular/core";
import { DatePipe, NgClass } from "@angular/common";
import { Task } from "../../models/task.model";
import { Subtask } from "../../models/subtask.model";

@Component({
  selector: "app-task-item",
  standalone: true,
  imports: [NgClass, DatePipe],
  templateUrl: "./task-item.component.html",
  styleUrl: "./task-item.component.scss"
})
export class TaskItemComponent {
  task = input.required<Task>();
  isLast = input<boolean>(false);

  taskToggled = output<number>();
  taskClicked = output<number>();
  taskEditRequested = output<number>();

  showMenu = signal<boolean>(false);

  taskProgress = computed(() => {
    const task = this.task();

    if (task.subtasks?.length) {
      const done = task.subtasks.filter(s => this.isSubtaskDone(s)).length;
      return Math.round((done / task.subtasks.length) * 100);
    }

    return task.completedAt ? 100 : 0;
  });

  progressClass = computed(() => {
    const pct = this.taskProgress();
    if (pct === 100) return "progress-100";
    if (pct === 0) return "progress-0";
    return "progress-default";
  });

  statusIcon = computed(() => {
    const pct = this.taskProgress();
    if (pct === 100) return "fa-check";
    if (pct === 0) return "fa-times";
    return "fa-question";
  });

  isTaskOverdue = computed(() => {
    const task = this.task();
    if (!task.dueDate || task.completedAt) return false;
    return new Date(task.dueDate) < new Date();
  });

  isSubtaskDone(subtask: Subtask): boolean {
    return subtask.completedAt != null;
  }

  isSubtaskOverdue(subtask: Subtask): boolean {
    if (!subtask.dueDate || subtask.completedAt) return false;
    return new Date(subtask.dueDate) < new Date();
  }

  toggleMenu(event: Event) {
    event.stopPropagation();
    this.showMenu.update(value => !value);
  }

  onToggleTask() {
    this.taskToggled.emit(this.task().taskId);
  }

  onClickTask() {
    this.taskClicked.emit(this.task().taskId);
  }

  onMarkComplete() {
    this.taskToggled.emit(this.task().taskId);
  }

  onToggleSubtask(index: number) {
    this.taskToggled.emit(this.task().taskId);
  }

  onEdit() {
    this.showMenu.set(false);
    this.taskEditRequested.emit(this.task().taskId);
  }

  onDelete() {
    this.showMenu.set(false);
  }

  onAddSubtask() {
    this.showMenu.set(false);
    this.taskToggled.emit(this.task().taskId);
  }
}
