import { Component, inject, effect, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { ProjectListComponent } from '../project-list/project-list.component';
import { ProjectDetailComponent } from '../project-detail/project-detail.component';
import { TaskFacade } from '../../facade/task.facade';
import { Company } from '../../models/company.enum';
import { ProjectDto } from '../../../../core/services/project-api.service';
import { Dialog } from '@angular/cdk/dialog';
import { SaveProjectDialogComponent, SaveProjectDialogData, SaveProjectDialogResult } from '../save-project-dialog/save-project-dialog.component';
import { SaveTaskDialogComponent, SaveTaskDialogData, SaveTaskDialogResult } from '../save-task-dialog/save-task-dialog.component';

@Component({
  selector: 'app-tasks-page',
  standalone: true,
  imports: [CommonModule, ProjectListComponent, ProjectDetailComponent],
  templateUrl: './tasks-page.html',
  styleUrl: './tasks-page.scss',
})
export class TasksPage {
  // Inject dependencies
  facade = inject(TaskFacade);
  private router = inject(Router);
  private route = inject(ActivatedRoute);
  #dialog = inject(Dialog);

  // Expose signals from facade
  projects = this.facade.projects;
  tasks = this.facade.tasks;
  isLoadingProjects = this.facade.isLoadingProjects;
  isLoadingTasks = this.facade.isLoadingTasks;
  companyFilter = this.facade.companyFilter;

  // Local state
  selectedProjectId = signal<number | null>(null);
  showOnlyMyTasks = signal<boolean>(false);
  
  // Computed signals
  selectedProject = computed(() => {
    const id = this.selectedProjectId();
    return id ? this.projects().find(p => p.id === id) : undefined;
  });

  // Expose enum to template
  readonly Company = Company;

  // Local loading state to distinguish initial project selection vs task filter reloads
  projectDetailLoading = signal<boolean>(false);

  // Track if we've done the initial load
  private hasInitiallyLoaded = false;

  constructor() {
    // Initial load of projects only if not already loaded (avoid flicker on first click)
    if (this.projects().length === 0 && !this.hasInitiallyLoaded) {
      this.hasInitiallyLoaded = true;
      this.facade.getProjects();
    }

    // Subscribe to route params and update selected project + load tasks
    this.route.paramMap.subscribe(params => {
      const projectId = params.get('id');
      if (projectId) {
        const idNum = Number(projectId);
        // Mark project detail as loading when navigating/selecting a project
        this.projectDetailLoading.set(true);
        this.selectedProjectId.set(idNum);
        // Reset task visibility filter when switching projects
        this.showOnlyMyTasks.set(false);
        this.facade.getTasks(idNum, false);
      } else {
        this.selectedProjectId.set(null);
        this.facade.clearTasks();
        this.projectDetailLoading.set(false);
      }
    });

    // When tasks finish loading, clear the project detail loading state
    effect(() => {
      const loading = this.isLoadingTasks();
      if (!loading) {
        this.projectDetailLoading.set(false);
      }
    });
  }

  onProjectSelected(projectId: number) {
    // Navigate to the project route
    console.log('Navigating to project:', projectId);
    this.router.navigate(['/taken/project', projectId]).then(
      success => console.log('Navigation success:', success),
      error => console.error('Navigation error:', error)
    );
  }

  onProjectCreate(project: ProjectDto) {
    // Remove temp id if present (dialog used Date.now()) and let facade assign
    const { id: _temp, ...rest } = project;
    this.facade.createProject(rest);
  }

  onProjectUpdate(project: ProjectDto) {
    this.facade.updateProject(project);
    // If updating currently selected project refresh selection reference
    if (this.selectedProjectId() === project.id) {
      // Trigger recompute by setting id again
      this.selectedProjectId.set(project.id);
    }
  }

  onNewProject() {
    const ref = this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: 'create' } as SaveProjectDialogData,
      backdropClass: ['fixed', 'inset-0', 'bg-black/40'],
      panelClass: ['dialog-panel', 'flex', 'items-center', 'justify-center']
    });
    ref.closed.subscribe(result => {
      if (result?.mode === 'create') {
        const { id: _tmp, ...rest } = result.project;
        this.facade.createProject(rest);
      }
    });
  }

  onEditProjectRequested() {
    const proj = this.selectedProject();
    if (!proj) return;
    const ref = this.#dialog.open<SaveProjectDialogResult>(SaveProjectDialogComponent, {
      data: { mode: 'update', project: proj } as SaveProjectDialogData,
      backdropClass: ['fixed', 'inset-0', 'bg-black/40'],
      panelClass: ['dialog-panel', 'flex', 'items-center', 'justify-center']
    });
    ref.closed.subscribe(result => {
      if (result?.mode === 'update') {
        this.facade.updateProject(result.project);
      }
    });
  }


  onTaskClicked(taskId: number) {
    const task = this.tasks().find(t => t.id === taskId);
    const proj = this.selectedProject();
    if (!task || !proj) return;
    const ref = this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: 'update', projectId: proj.id, task } as SaveTaskDialogData,
      backdropClass: ['fixed', 'inset-0', 'bg-black/40'],
      panelClass: ['dialog-panel', 'flex', 'items-center', 'justify-center']
    });
    ref.closed.subscribe(result => {
      if (result?.mode === 'update') {
        this.facade.updateTask(result.task);
      }
    });
  }

  onTaskFilterToggled(showOnlyMine: boolean) {
    this.showOnlyMyTasks.set(showOnlyMine);
    const pid = this.selectedProjectId();
    if (pid) {
      this.facade.getTasks(pid, showOnlyMine);
    }
  }

  onTaskToggled(taskId: number) {
    console.log('Task toggled:', taskId);
    // Hook for future state update if persisting toggles
  }

  filterCompany(filter: 'ALL' | Company) {
    // Clear current project selection and route when changing company filter
    this.selectedProjectId.set(null);
    this.router.navigate(['/taken']).catch(() => {});
    this.facade.setCompanyFilter(filter);
  }

  onCreateTaskRequested() {
    const proj = this.selectedProject();
    if (!proj) return;
    const ref = this.#dialog.open<SaveTaskDialogResult>(SaveTaskDialogComponent, {
      data: { mode: 'create', projectId: proj.id } as SaveTaskDialogData,
      backdropClass: ['fixed', 'inset-0', 'bg-black/40'],
      panelClass: ['dialog-panel', 'flex', 'items-center', 'justify-center']
    });
    ref.closed.subscribe(result => {
      if (result?.mode === 'create') {
        const { id: _tmp, ...rest } = result.task;
        this.facade.createTask(rest);
      }
    });
  }
}
