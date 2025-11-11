import { Component, computed, input, output, signal } from "@angular/core";
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
  taskEditRequested = output<number>();

  // Local state as signals
  showSubtasks = signal<boolean>(false);
  showMenu = signal<boolean>(false);

  // Helper method to check if subtask is done (public for template use)
  isSubtaskDone(subtask: Subtask): boolean {
    return subtask.completedAt != null;
  }

  // Computed signals
  taskProgress = computed(() => {
    const task = this.task();
    if (task.subtasks && task.subtasks.length > 0) {
      const done = task.subtasks.filter(s => this.isSubtaskDone(s)).length;
      return Math.round((done / task.subtasks.length) * 100);
    }
    if (typeof task.subtaskCount === "number" && typeof task.completedSubtaskCount === "number" && task.subtaskCount > 0) {
      return Math.round((task.completedSubtaskCount / task.subtaskCount) * 100);
    }
    return task.completedAt != null ? 100 : 0;
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

  actionButtonText = computed(() => {
    return this.taskProgress() === 0 ? "Ik ben er mee bezig" : "Klaar";
  });

  subtasksDone = computed(() => {
    return this.task().subtasks?.filter(s => this.isSubtaskDone(s)).length ?? 0;
  });

  subtasksTotal = computed(() => {
    return this.task().subtasks?.length ?? 0;
  });

  lastSubtaskDeadline = computed(() => {
    const task = this.task();
    if (!task.subtasks || task.subtasks.length === 0) return "";
    return task.subtasks[task.subtasks.length - 1]?.dueDate ?? "";
  });

  private monthMap: Record<string, number> = {
    "januari": 0,
    "februari": 1,
    "maart": 2,
    "april": 3,
    "mei": 4,
    "juni": 5,
    "juli": 6,
    "augustus": 7,
    "september": 8,
    "oktober": 9,
    "november": 10,
    "december": 11
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
    if (!dateStr) return "";
    const d = this.parseFlexible(dateStr);
    if (!d) return dateStr; // already localized text
    const fmt = new Intl.DateTimeFormat("nl-NL", { day: "numeric", month: "long", year: "numeric" });
    return fmt.format(d);
  }

  isTaskDeadlineOverdue = computed(() => {
    const task = this.task();
    if (!task.dueDate || task.completedAt != null) return false;
    const d = this.parseFlexible(task.dueDate);
    if (!d) return false;
    const today = new Date();
    today.setHours(0, 0, 0, 0);
    return d.getTime() < today.getTime();
  });

  isSubtaskOverdue(subtask: Subtask): boolean {
    if (!subtask.dueDate || subtask.completedAt != null) return false;
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
    this.taskToggled.emit(this.task().taskId);
  }

  onClickTask() {
    this.taskClicked.emit(this.task().taskId);
  }

  onMarkComplete() {
    // Emit toggle to trigger status update via facade
    this.taskToggled.emit(this.task().taskId);
  }

  onToggleSubtask(index: number) {
    // Emit toggle to trigger subtask update via facade
    this.taskToggled.emit(this.task().taskId);
  }

  onEdit() {
    this.showMenu.set(false);
    this.taskEditRequested.emit(this.task().taskId);
  }

  onDelete() {
    this.showMenu.set(false);
    // TODO: Implement delete functionality
  }

  onAddSubtask() {
    this.showMenu.set(false);
    // TODO: Implement add subtask via facade
    this.taskToggled.emit(this.task().taskId);
  }

  private formatTodayDutch(): string {
    const months = ["januari", "februari", "maart", "april", "mei", "juni", "juli", "augustus", "september", "oktober", "november", "december"];
    const d = new Date();
    return `${d.getDate()} ${months[d.getMonth()]} ${d.getFullYear()}`;
  }
}
