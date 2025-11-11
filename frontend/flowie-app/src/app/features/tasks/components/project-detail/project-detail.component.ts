import { Component, input, output } from "@angular/core";
import { Company } from "../../models/company.enum";
import { TaskItemComponent } from "../task-item/task-item.component";
import { Project } from "../../models/project.model";
import { Task } from "../../models/task.model";

@Component({
  selector: "app-project-detail",
  standalone: true,
  imports: [TaskItemComponent],
  templateUrl: "./project-detail.component.html",
  styleUrl: "./project-detail.component.scss"
})
export class ProjectDetailComponent {
  project = input.required<Project>();
  tasks = input<Task[]>([]);
  isLoadingTasks = input<boolean>(false);
  isDetailLoading = input<boolean>(false);
  showOnlyMyTasks = input<boolean>(false);

  // Outputs as signals
  taskFilterToggled = output<boolean>();
  taskClicked = output<number>();
  taskToggled = output<number>();
  taskEditRequested = output<number>();
  projectEditRequested = output<void>();
  taskCreateRequested = output<void>();

  // Expose enum to template
  readonly Company = Company;

  onToggleFilter(val: boolean) {
    this.taskFilterToggled.emit(val);
  }

  onTaskClicked(id: number) {
    this.taskClicked.emit(id);
  }

  onTaskToggled(id: number) {
    this.taskToggled.emit(id);
  }

  onTaskEdit(id: number) {
    this.taskEditRequested.emit(id);
  }

  onEditProject() {
    this.projectEditRequested.emit();
  }

  onCreateTask() {
    this.taskCreateRequested.emit();
  }
}
