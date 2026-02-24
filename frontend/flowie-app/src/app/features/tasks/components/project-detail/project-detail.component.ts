import { Component, input, OnChanges, output, signal } from "@angular/core";
import { Company } from "../../models/company.enum";
import { TaskItemComponent } from "../task-item/task-item.component";
import { Project } from "../../models/project.model";
import { Task } from "../../models/task.model";
import { TaskStatus } from "../../models/task-status.enum";
import { CdkDragDrop, CdkDropList, CdkDrag, moveItemInArray } from "@angular/cdk/drag-drop";

@Component({
  selector: "app-project-detail",
  standalone: true,
  imports: [TaskItemComponent, CdkDropList, CdkDrag],
  templateUrl: "./project-detail.component.html",
  styleUrl: "./project-detail.component.scss"
})
export class ProjectDetailComponent implements OnChanges {
  readonly Company = Company;

  project = input.required<Project>();
  tasks = input<Task[]>([]);
  isLoadingTasks = input<boolean>(false);
  isDetailLoading = input<boolean>(false);
  showOnlyMyTasks = input<boolean>(false);

  taskFilterToggled = output<boolean>();
  projectUpdateRequested = output<void>();
  taskCreateRequested = output<void>();
  taskUpdateRequested = output<number>();
  taskDeleteRequested = output<number>();
  taskStatusChanged = output<{ taskId: number; status: TaskStatus }>();
  subtaskCreateRequested = output<number>();
  subtaskUpdateRequested = output<number>();
  subtaskDeleteRequested = output<number>();
  subtaskStatusChanged = output<{ taskId: number; status: TaskStatus }>();
  taskReorderRequested = output<{ taskId: number; displayOrder: number }[]>();
  subtaskReorderRequested = output<{ taskId: number; displayOrder: number }[]>();

  orderedTasks = signal<Task[]>([]);

  ngOnChanges() {
    this.orderedTasks.set([...this.tasks()]);
  }

  onTaskDrop(event: CdkDragDrop<Task[]>) {
    const tasks = [...this.orderedTasks()];
    moveItemInArray(tasks, event.previousIndex, event.currentIndex);
    this.orderedTasks.set(tasks);
    this.taskReorderRequested.emit(tasks.map((t, i) => ({ taskId: t.taskId, displayOrder: i })));
  }

  onToggleTaskFilter(val: boolean) {
    this.taskFilterToggled.emit(val);
  }

  onUpdateProject() {
    this.projectUpdateRequested.emit();
  }

  onCreateTask() {
    this.taskCreateRequested.emit();
  }

  onTaskUpdate(id: number) {
    this.taskUpdateRequested.emit(id);
  }

  onTaskDelete(id: number) {
    this.taskDeleteRequested.emit(id);
  }

  onTaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    this.taskStatusChanged.emit(event);
  }

  onSubtaskCreate(taskId: number) {
    this.subtaskCreateRequested.emit(taskId);
  }

  onSubtaskUpdate(subtaskId: number) {
    this.subtaskUpdateRequested.emit(subtaskId);
  }

  onSubtaskDelete(subtaskId: number) {
    this.subtaskDeleteRequested.emit(subtaskId);
  }

  onSubtaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    this.subtaskStatusChanged.emit(event);
  }

  onSubtaskReorder(items: { taskId: number; displayOrder: number }[]) {
    this.subtaskReorderRequested.emit(items);
  }
}
