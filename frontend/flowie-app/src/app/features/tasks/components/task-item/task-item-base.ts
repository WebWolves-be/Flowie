import { Directive, computed, input, OnChanges, output, signal } from "@angular/core";
import { Task } from "../../models/task.model";
import { Subtask } from "../../models/subtask.model";
import { TaskStatus } from "../../models/task-status.enum";
import { CdkDragDrop, moveItemInArray } from "@angular/cdk/drag-drop";

/**
 * Everything a task row does that is not layout.
 *
 * Mobile and desktop render genuinely different designs — a scannable row with a
 * detail sheet versus an expandable card — so they get their own templates. The
 * status maths, subtask ordering and event plumbing are identical either way and
 * live here, so a fix cannot land on one platform and be forgotten on the other.
 */
@Directive()
export abstract class TaskItemBase implements OnChanges {
  task = input.required<Task>();
  isLast = input<boolean>(false);

  taskUpdateRequested = output<number>();
  taskDeleteRequested = output<number>();
  taskStatusChanged = output<{ taskId: number; status: TaskStatus }>();

  subtaskCreateRequested = output<number>();
  subtaskUpdateRequested = output<number>();
  subtaskDeleteRequested = output<number>();
  subtaskStatusChanged = output<{ taskId: number; status: TaskStatus }>();
  subtaskReorderRequested = output<{ taskId: number; displayOrder: number }[]>();

  showMenu = signal<boolean>(false);
  showSubtaskMenu = signal<number | null>(null);
  orderedSubtasks = signal<Subtask[]>([]);
  expanded = signal<boolean>(true);

  ngOnChanges() {
    this.orderedSubtasks.set([...(this.task().subtasks ?? [])]);
    this.expanded.set(!this.isDone());
  }

  toggleExpanded() {
    if (this.isDone()) {
      this.expanded.update(v => !v);
    }
  }

  onSubtaskDrop(event: CdkDragDrop<Subtask[]>) {
    const subtasks = [...this.orderedSubtasks()];
    moveItemInArray(subtasks, event.previousIndex, event.currentIndex);
    this.orderedSubtasks.set(subtasks);
    this.subtaskReorderRequested.emit(subtasks.map((s, i) => ({ taskId: s.taskId, displayOrder: i })));
  }

  hasSubtasks = computed(() => (this.task().subtasks?.length ?? 0) > 0);

  /**
   * A task that has subtasks takes its status from them, not from its own
   * column: it is only done once every subtask is done. Its own status is
   * whatever it happened to be before the subtasks existed, so trusting it
   * showed a finished task the moment a single subtask went green.
   */
  effectiveStatus = computed<TaskStatus>(() => {
    const task = this.task();
    const subtasks = task.subtasks ?? [];
    if (subtasks.length === 0) return task.status;

    if (subtasks.every(subtask => this.isSubtaskDone(subtask))) return TaskStatus.Done;
    if (subtasks.some(subtask => subtask.status === TaskStatus.WaitingOn)) return TaskStatus.WaitingOn;
    if (subtasks.some(subtask => subtask.status !== TaskStatus.Pending)) return TaskStatus.Ongoing;
    return TaskStatus.Pending;
  });

  isPending = computed(() => this.effectiveStatus() === TaskStatus.Pending);
  isOngoing = computed(() => this.effectiveStatus() === TaskStatus.Ongoing);
  isDone = computed(() => this.effectiveStatus() === TaskStatus.Done);
  isWaitingOn = computed(() => this.effectiveStatus() === TaskStatus.WaitingOn);

  taskProgress = computed(() => {
    const task = this.task();

    if (task.subtasks?.length) {
      const done = task.subtasks.filter(s => this.isSubtaskDone(s)).length;
      return Math.round((done / task.subtasks.length) * 100);
    }

    return task.completedAt ? 100 : 0;
  });

  progressClass = computed(() => {
    const status = this.effectiveStatus();
    if (status === TaskStatus.Done) return "progress-100";
    if (status === TaskStatus.Pending) return "progress-0";
    if (status === TaskStatus.WaitingOn) return "progress-waiting";
    return "progress-default";
  });

  statusIcon = computed(() => {
    const status = this.effectiveStatus();
    if (status === TaskStatus.Done) return "check";
    if (status === TaskStatus.Pending) return "pending";
    if (status === TaskStatus.WaitingOn) return "waiting";
    return "ongoing";
  });

  /** Dutch label for the current status, used where there is no room for buttons. */
  statusLabel = computed(() => {
    const status = this.effectiveStatus();
    if (status === TaskStatus.Done) return "Klaar";
    if (status === TaskStatus.Pending) return "Openstaand";
    if (status === TaskStatus.WaitingOn) return "Wachten op";
    return "Bezig";
  });

  /**
   * The status one tap forward: openstaand -> bezig -> klaar, and klaar back to
   * openstaand. Tasks with subtasks derive their status from those, so tapping
   * does nothing there.
   */
  nextStatus = computed<TaskStatus | null>(() => {
    if (this.hasSubtasks()) return null;

    switch (this.task().status) {
      case TaskStatus.Pending:
        return TaskStatus.Ongoing;
      case TaskStatus.Ongoing:
      case TaskStatus.WaitingOn:
        return TaskStatus.Done;
      case TaskStatus.Done:
        return TaskStatus.Pending;
      default:
        return null;
    }
  });

  advanceStatus() {
    const next = this.nextStatus();
    if (next === null) return;
    this.taskStatusChanged.emit({ taskId: this.task().taskId, status: next });
  }

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
