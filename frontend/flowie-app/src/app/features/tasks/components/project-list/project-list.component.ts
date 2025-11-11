import { Component, input, output } from "@angular/core";
import { Project } from "../../models/project.model";
import { Company } from "../../models/company.enum";
import { DecimalPipe } from "@angular/common";

@Component({
  selector: "app-project-list",
  standalone: true,
  imports: [
    DecimalPipe
  ],
  templateUrl: "./project-list.component.html",
  styleUrl: "./project-list.component.scss"
})
export class ProjectListComponent {
  readonly Company = Company;

  projects = input<Project[]>([]);
  loading = input<boolean>(false);
  selectedProjectId = input<number>();

  projectSelected = output<number>();

  onSelectProject(projectId: number) {
    this.projectSelected.emit(projectId);
  }
}
