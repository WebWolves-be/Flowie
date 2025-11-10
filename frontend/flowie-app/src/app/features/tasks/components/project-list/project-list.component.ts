import { Component, input, output, inject } from "@angular/core";
import { Dialog } from '@angular/cdk/dialog';
import { SaveProjectDialogComponent, SaveProjectDialogData, SaveProjectDialogResult } from '../save-project-dialog/save-project-dialog.component';
import { ProjectDto } from "../../../../core/services/project-api.service";
import { Company } from "../../models/company.enum";

@Component({
  selector: "app-project-list",
  standalone: true,
  imports: [],
  templateUrl: "./project-list.component.html",
  styleUrl: "./project-list.component.scss"
})
export class ProjectListComponent {
  projects = input<ProjectDto[]>([]);
  selectedProjectId = input<number>();
  loading = input<boolean>(false);
  projectSelected = output<number>();
  projectCreateRequested = output<ProjectDto>();
  projectUpdateRequested = output<ProjectDto>();

  // Expose enum to template
  readonly Company = Company;

  #dialog = inject(Dialog);

  openCreateProjectDialog() {
    const ref = this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: 'create' } as SaveProjectDialogData,
      backdropClass: ['fixed', 'inset-0', 'bg-black/40'],
      panelClass: ['dialog-panel', 'flex', 'items-center', 'justify-center']
    });
    ref.closed.subscribe(result => {
      if (result?.mode === 'create') {
        this.projectCreateRequested.emit(result.project);
      }
    });
  }

  openEditProjectDialog(project: ProjectDto) {
    const ref = this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: 'update', project } as SaveProjectDialogData,
      backdropClass: ['fixed', 'inset-0', 'bg-black/40'],
      panelClass: ['dialog-panel', 'flex', 'items-center', 'justify-center']
    });
    ref.closed.subscribe(result => {
      if (result?.mode === 'update') {
        this.projectUpdateRequested.emit(result.project);
      }
    });
  }

  onSelectProject(projectId: number) {
    this.projectSelected.emit(projectId);
  }
}
