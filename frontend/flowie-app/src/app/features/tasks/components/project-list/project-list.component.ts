import { Component, input, output } from "@angular/core";
import { Project } from "../../models/project.model";
import { Company, CompanyDisplayLabels } from "../../models/company.enum";
import { DecimalPipe, NgClass } from "@angular/common";

@Component({
  selector: "app-project-list",
  standalone: true,
  imports: [
    DecimalPipe,
    NgClass
  ],
  templateUrl: "./project-list.component.html",
  styleUrl: "./project-list.component.scss"
})
export class ProjectListComponent {
  readonly Company = Company;
  readonly CompanyDisplayLabels = CompanyDisplayLabels;

  projects = input<Project[]>([]);
  loading = input<boolean>(false);
  selectedProjectId = input<number>();

  projectSelected = output<number>();

  onSelectProject(projectId: number) {
    this.projectSelected.emit(projectId);
  }
}
