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

  projectEditRequested = output<void>();

  taskCreateRequested = output<void>();
  taskEditRequested = output<number>();
  taskDeleteRequested = output<number>();
  taskStatusChanged = output<{ taskId: number; status: TaskStatus }>();

  subtaskAddRequested = output<number>();
  subtaskEditRequested = output<number>();
  subtaskDeleteRequested = output<number>();
  subtaskStatusChanged = output<{ taskId: number; status: TaskStatus }>();

  onToggleTaskFilter(val: boolean) {
    this.taskFilterToggled.emit(val);
  }

  onEditProject() {
    this.projectEditRequested.emit();
  }

  onCreateTask() {
    this.taskCreateRequested.emit();
  }

  onTaskEdit(id: number) {
    this.taskEditRequested.emit(id);
  }

  onTaskDelete(id: number) {
    this.taskDeleteRequested.emit(id);
  }

  onTaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    this.taskStatusChanged.emit(event);
  }

  onSubtaskAdd(taskId: number) {
    this.subtaskAddRequested.emit(taskId);
  }

  onSubtaskEdit(subtaskId: number) {
    this.subtaskEditRequested.emit(subtaskId);
  }

  onSubtaskDelete(subtaskId: number) {
    this.subtaskDeleteRequested.emit(subtaskId);
  }

  onSubtaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    this.subtaskStatusChanged.emit(event);
  }
}
