import { Component, input, output, signal, computed } from "@angular/core";
import { NgClass } from "@angular/common";
import { TaskDto, SubtaskDto } from "../../../../core/services/task-api.service";

@Component({
  selector: "app-task-item",
  standalone: true,
  imports: [NgClass],
  templateUrl: "./task-item.component.html",
  styleUrl: "./task-item.component.scss"
})
export class TaskItemComponent {
  // Inputs as signals
  task = input.required<TaskDto>();
  isLast = input<boolean>(false);
  
  // Outputs as signal-based outputs
  taskToggled = output<number>();
  taskClicked = output<number>();
  taskEditRequested = output<number>();

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
    if (typeof task.subtaskCount === 'number' && typeof task.completedSubtaskCount === 'number' && task.subtaskCount > 0) {
      return Math.round((task.completedSubtaskCount / task.subtaskCount) * 100);
    }
    return typeof task.progress === 'number' ? task.progress : (task.statusName === 'Done' ? 100 : 0);
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
    return task.subtasks[task.subtasks.length - 1]?.dueDate ?? '';
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

  private parseFlexible(dateStr: string): Date | null {
    if (!dateStr) return null;
    const iso = new Date(dateStr);
    if (!isNaN(iso.getTime())) return iso;
    return this.parseDutchDate(dateStr);
  }

  formatDutch(dateStr: string | null | undefined): string {
    if (!dateStr) return '';
    const d = this.parseFlexible(dateStr);
    if (!d) return dateStr; // already localized text
    const fmt = new Intl.DateTimeFormat('nl-NL', { day: 'numeric', month: 'long', year: 'numeric' });
    return fmt.format(d);
  }

  isTaskDeadlineOverdue = computed(() => {
    const task = this.task();
    if (!task.dueDate || (task.statusName === 'Done' || this.taskProgress() === 100)) return false;
    const d = this.parseFlexible(task.dueDate);
    if (!d) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return d.getTime() < today.getTime();
  });

  isSubtaskOverdue(subtask: SubtaskDto): boolean {
    if (!subtask.dueDate || subtask.done) return false;
    const d = this.parseFlexible(subtask.dueDate);
    if (!d) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return d.getTime() < today.getTime();
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
    task.statusName = 'Done';
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
    this.taskEditRequested.emit(this.task().id);
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
      assignee: { name: task.assignee.name },
      dueDate: this.formatTodayDutch(),
      done: false,
      status: task.status, // inherit status for now
      statusName: task.statusName
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
