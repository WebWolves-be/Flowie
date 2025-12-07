import { Component, input, output } from "@angular/core";
import { Company } from "../../models/company.enum";
import { TaskItemComponent } from "../task-item/task-item.component";
import { Project } from "../../models/project.model";
import { Task } from "../../models/task.model";
import { TaskStatus } from "../../models/task-status.enum";

@Component({
  selector: "app-project-detail",
  standalone: true,
  imports: [TaskItemComponent],
  templateUrl: "./project-detail.component.html",
  styleUrl: "./project-detail.component.scss"
})
export class ProjectDetailComponent {
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
}
