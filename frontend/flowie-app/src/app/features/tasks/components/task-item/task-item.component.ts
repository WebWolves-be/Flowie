import { Component, input, output, signal, computed } from "@angular/core";
import { NgClass } from "@angular/common";
import { Task } from "../../models/task.model";
import { Subtask } from "../../models/subtask.model";

@Component({
  selector: "app-task-item",
  standalone: true,
  imports: [NgClass],
  templateUrl: "./task-item.component.html",
  styleUrl: "./task-item.component.scss"
})
export class TaskItemComponent {
  // Inputs as signals
  task = input.required<Task>();
  isLast = input<boolean>(false);
  
  // Outputs as signal-based outputs
  taskToggled = output<number>();
  taskClicked = output<number>();

  // Local state as signals
  showSubtasks = signal<boolean>(false);
  showMenu = signal<boolean>(false);

  // Computed signals
  taskProgress = computed(() => {
    const task = this.task();
    if (task.subtasks && task.subtasks.length > 0) {
      const done = task.subtasks.filter(s => s.done).length;
      return Math.round((done / task.subtasks.length) * 100);
    }
    return task.progress ?? (task.completed ? 100 : 0);
  });

  progressClass = computed(() => {
    const pct = this.taskProgress();
    if (pct === 100) return 'progress-100';
    if (pct === 0) return 'progress-0';
    return 'progress-default';
  });

  statusIcon = computed(() => {
    const pct = this.taskProgress();
    if (pct === 100) return 'fa-check';
    if (pct === 0) return 'fa-times';
    return 'fa-question';
  });

  actionButtonText = computed(() => {
    return this.taskProgress() === 0 ? 'Ik ben er mee bezig' : 'Klaar';
  });

  subtasksDone = computed(() => {
    return this.task().subtasks?.filter(s => s.done).length ?? 0;
  });

  subtasksTotal = computed(() => {
    return this.task().subtasks?.length ?? 0;
  });

  lastSubtaskDeadline = computed(() => {
    const task = this.task();
    if (!task.subtasks || task.subtasks.length === 0) return '';
    return task.subtasks[task.subtasks.length - 1]?.deadline ?? '';
  });

  private monthMap: Record<string, number> = {
    'januari': 0,
    'februari': 1,
    'maart': 2,
    'april': 3,
    'mei': 4,
    'juni': 5,
    'juli': 6,
    'augustus': 7,
    'september': 8,
    'oktober': 9,
    'november': 10,
    'december': 11
  };

  private parseDutchDate(dateStr: string): Date | null {
    if (!dateStr) return null;
    const m = dateStr.trim().toLowerCase().match(/^(\d{1,2})\s+([a-zA-Zèéëïäöü]+)\s+(\d{4})$/);
    if (!m) return null;
    const day = parseInt(m[1], 10);
    const monthName = m[2];
    const year = parseInt(m[3], 10);
    const monthIndex = this.monthMap[monthName];
    if (monthIndex === undefined) return null;
    return new Date(year, monthIndex, day);
  }

  isTaskDeadlineOverdue = computed(() => {
    const task = this.task();
    if (!task.deadline || task.completed) return false;
    const d = this.parseDutchDate(task.deadline);
    if (!d) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return d.getTime() < today.getTime();
  });

  isSubtaskOverdue(subtask: Subtask): boolean {
    if (!subtask.deadline || subtask.done) return false;
    const d = this.parseDutchDate(subtask.deadline);
    if (!d) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return d.getTime() < today.getTime();
  }

  getInitials(name: string): string {
    if (!name) return '';
    const parts = name.trim().split(/\s+/);
    if (parts.length === 1) return parts[0].charAt(0).toUpperCase();
    const first = parts[0].charAt(0).toUpperCase();
    const last = parts[parts.length - 1].charAt(0).toUpperCase();
    return `${first}${last}`;
  }

  toggleSubtasks() {
    this.showSubtasks.update(value => !value);
  }

  toggleMenu(event: Event) {
    event.stopPropagation();
    this.showMenu.update(value => !value);
  }

  onToggleTask() {
    this.taskToggled.emit(this.task().id);
  }

  onClickTask() {
    this.taskClicked.emit(this.task().id);
  }

  onMarkComplete() {
    // Mark task and all subtasks as complete
    const task = this.task();
    task.completed = true;
    task.progress = 100;
    if (task.subtasks) {
      task.subtasks.forEach(s => s.done = true);
    }
    this.taskToggled.emit(task.id);
  }

  onToggleSubtask(index: number) {
    const task = this.task();
    if (task.subtasks) {
      task.subtasks[index].done = !task.subtasks[index].done;
      this.taskToggled.emit(task.id);
    }
  }

  onEdit() {
    this.showMenu.set(false);
    // TODO: Implement edit functionality
  }

  onDelete() {
    this.showMenu.set(false);
    // TODO: Implement delete functionality
  }

  onAddSubtask() {
    this.showMenu.set(false);
    const task = this.task();
    if (!task.subtasks) task.subtasks = [];
    task.subtasks.push({
      title: 'Nieuwe subtaak',
      assignee: task.assignee.name,
      deadline: this.formatTodayDutch(),
      done: false
    });
    // Emit toggle to trigger any parent refresh logic (reuse taskToggled)
    this.taskToggled.emit(task.id);
  }

  private formatTodayDutch(): string {
    const months = ['januari','februari','maart','april','mei','juni','juli','augustus','september','oktober','november','december'];
    const d = new Date();
    return `${d.getDate()} ${months[d.getMonth()]} ${d.getFullYear()}`;
  }
}
