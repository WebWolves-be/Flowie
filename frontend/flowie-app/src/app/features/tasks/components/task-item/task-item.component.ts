import { Component, computed, input, output, signal } from "@angular/core";
import { DatePipe, NgClass } from "@angular/common";
import { Task } from "../../models/task.model";
import { Subtask } from "../../models/subtask.model";
import { TaskStatus } from "../../models/task-status.enum";

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

  taskUpdateRequested = output<number>();
  taskDeleteRequested = output<number>();
  taskStatusChanged = output<{ taskId: number; status: TaskStatus }>();

  subtaskCreateRequested = output<number>();
  subtaskUpdateRequested = output<number>();
  subtaskDeleteRequested = output<number>();
  subtaskStatusChanged = output<{ taskId: number; status: TaskStatus }>();

  showMenu = signal<boolean>(false);
  showSubtaskMenu = signal<number | null>(null);

  isPending = computed(() => this.task().status === TaskStatus.Pending);
  isOngoing = computed(() => this.task().status === TaskStatus.Ongoing);
  isDone = computed(() => this.task().status === TaskStatus.Done);
  isWaitingOn = computed(() => this.task().status === TaskStatus.WaitingOn);

  taskProgress = computed(() => {
    const task = this.task();

    if (task.subtasks?.length) {
      const done = task.subtasks.filter(s => this.isSubtaskDone(s)).length;
      return Math.round((done / task.subtasks.length) * 100);
    }

    return task.completedAt ? 100 : 0;
  });

  progressClass = computed(() => {
    const status = this.task().status;
    if (status === TaskStatus.Done) return "progress-100";
    if (status === TaskStatus.Pending) return "progress-0";
    if (status === TaskStatus.WaitingOn) return "progress-waiting";
    return "progress-default";
  });

  statusIcon = computed(() => {
    const status = this.task().status;
    if (status === TaskStatus.Done) return "fa-check";
    if (status === TaskStatus.Pending) return "fa-times";
    if (status === TaskStatus.WaitingOn) return "fa-clock";
    return "fa-question";
  });

  isTaskOverdue = computed(() => {
    const task = this.task();
    if (!task.dueDate || task.completedAt) return false;
    return new Date(task.dueDate) < new Date();
  });

  isSubtaskDone(subtask: Subtask): boolean {
    return subtask.status === TaskStatus.Done;
  }

  isSubtaskPending(subtask: Subtask): boolean {
    return subtask.status === TaskStatus.Pending;
  }

  isSubtaskOngoing(subtask: Subtask): boolean {
    return subtask.status === TaskStatus.Ongoing;
  }

  isSubtaskWaitingOn(subtask: Subtask): boolean {
    return subtask.status === TaskStatus.WaitingOn;
  }

  isSubtaskOverdue(subtask: Subtask): boolean {
    if (!subtask.dueDate || subtask.completedAt) return false;
    return new Date(subtask.dueDate) < new Date();
  }

  toggleTaskMenu(event: Event) {
    event.stopPropagation();
    this.showMenu.update(value => !value);
  }

  onUpdateTask() {
    this.showMenu.set(false);
    this.taskUpdateRequested.emit(this.task().taskId);
  }

  onDeleteTask() {
    this.showMenu.set(false);
    this.taskDeleteRequested.emit(this.task().taskId);
  }

  onCreateSubtask() {
    this.showMenu.set(false);
    this.subtaskCreateRequested.emit(this.task().taskId);
  }

  onStartTask() {
    this.taskStatusChanged.emit({ taskId: this.task().taskId, status: TaskStatus.Ongoing });
  }

  onCompleteTask() {
    this.taskStatusChanged.emit({ taskId: this.task().taskId, status: TaskStatus.Done });
  }

  onWaitTask() {
    this.taskStatusChanged.emit({ taskId: this.task().taskId, status: TaskStatus.WaitingOn });
  }

  onReopenTask() {
    this.taskStatusChanged.emit({ taskId: this.task().taskId, status: TaskStatus.Pending });
  }

  onStartSubtask(subtask: Subtask) {
    this.subtaskStatusChanged.emit({ taskId: subtask.taskId, status: TaskStatus.Ongoing });
  }

  onCompleteSubtask(subtask: Subtask) {
    this.subtaskStatusChanged.emit({ taskId: subtask.taskId, status: TaskStatus.Done });
  }

  onWaitSubtask(subtask: Subtask) {
    this.subtaskStatusChanged.emit({ taskId: subtask.taskId, status: TaskStatus.WaitingOn });
  }

  onReopenSubtask(subtask: Subtask) {
    this.subtaskStatusChanged.emit({ taskId: subtask.taskId, status: TaskStatus.Pending });
  }

  toggleSubtaskMenu(event: Event, subtaskId: number) {
    event.stopPropagation();
    this.showSubtaskMenu.update(current => current === subtaskId ? null : subtaskId);
  }

  onUpdateSubtask(subtask: Subtask) {
    this.showSubtaskMenu.set(null);
    this.subtaskUpdateRequested.emit(subtask.taskId);
  }

  onDeleteSubtask(subtask: Subtask) {
    this.showSubtaskMenu.set(null);
    this.subtaskDeleteRequested.emit(subtask.taskId);
  }

  waitingDays = computed(() => {
    const since = this.task().waitingSince;
    if (!since) return null;
    return Math.floor((Date.now() - new Date(since).getTime()) / 86_400_000);
  });

  subtaskWaitingDays(subtask: Subtask): number | null {
    if (!subtask.waitingSince) return null;
    return Math.floor((Date.now() - new Date(subtask.waitingSince).getTime()) / 86_400_000);
  }

  daysAgo(date: string | Date): string {
    const days = Math.floor((Date.now() - new Date(date).getTime()) / 86_400_000);
    if (days === 0) return "";
    return `(${days}d)`;
  }
}
