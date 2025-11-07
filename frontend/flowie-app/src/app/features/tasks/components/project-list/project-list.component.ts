import { Component, Input, Output, EventEmitter } from "@angular/core";
import { CommonModule } from "@angular/common";
import { Project } from "../../models/project.model";
import { Company } from "../../models/company.enum";

@Component({
  selector: "app-project-list",
  standalone: true,
  imports: [CommonModule],
  templateUrl: "./project-list.component.html",
  styleUrl: "./project-list.component.scss"
})
export class ProjectListComponent {
  @Input() projects: Project[] = [];
  @Input() selectedProjectId?: number;
  @Input() loading: boolean = false;
  @Output() projectSelected = new EventEmitter<number>();

  // Expose enum to template
  readonly Company = Company;

  onSelectProject(projectId: number) {
    this.projectSelected.emit(projectId);
  }
}
