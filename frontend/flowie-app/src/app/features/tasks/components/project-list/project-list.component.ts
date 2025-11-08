import { Component, input, output, inject } from "@angular/core";
import { Dialog } from '@angular/cdk/dialog';
import { SaveProjectDialogComponent, SaveProjectDialogData, SaveProjectDialogResult } from '../save-project-dialog/save-project-dialog.component';
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
  projectCreateRequested = output<Project>();
  projectUpdateRequested = output<Project>();

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

  openEditProjectDialog(project: Project) {
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
