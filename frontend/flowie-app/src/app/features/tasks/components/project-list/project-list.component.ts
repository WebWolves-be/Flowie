import { Component, input, output } from "@angular/core";
import { Project } from "../../models/project.model";
import { Company } from "../../models/company.enum";

@Component({
  selector: "app-project-list",
  standalone: true,
  imports: [],
  templateUrl: "./project-list.component.html",
  styleUrl: "./project-list.component.scss"
})
export class ProjectListComponent {
  projects = input<Project[]>([]);
  selectedProjectId = input<number>();
  loading = input<boolean>(false);
  projectSelected = output<number>();

  // Expose enum to template
  readonly Company = Company;

  onSelectProject(projectId: number) {
    this.projectSelected.emit(projectId);
  }
}
