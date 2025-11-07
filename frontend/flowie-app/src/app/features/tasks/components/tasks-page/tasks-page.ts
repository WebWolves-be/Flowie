import { Component, inject, effect, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { ProjectListComponent } from '../project-list/project-list.component';
import { ProjectDetailComponent } from '../project-detail/project-detail.component';
import { TaskFacade } from '../../facade/task.facade';
import { Company } from '../../models/company.enum';

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

  // Expose signals from facade
  projects = this.facade.projects;
  tasks = this.facade.tasks;
  isLoadingProjects = this.facade.isLoadingProjects;
  isLoadingTasks = this.facade.isLoadingTasks;

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

  // Local filter state for projects list
  companyFilter = signal<'ALL' | Company>('ALL');

  // Local loading state to distinguish initial project selection vs task filter reloads
  projectDetailLoading = signal<boolean>(false);

  constructor() {
    // Initial load of projects only if not already loaded (avoid flicker on first click)
    if (this.projects().length === 0) {
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
        this.facade.getTasks(idNum, this.showOnlyMyTasks());
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


  onTaskClicked(taskId: number) {
    console.log('Task clicked:', taskId);
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
    this.companyFilter.set(filter);
    // Clear current project selection and route when changing company filter
    this.selectedProjectId.set(null);
    this.router.navigate(['/taken']).catch(() => {});
    if (filter === 'ALL') {
      this.facade.getProjects();
    } else {
      this.facade.getProjects(filter);
    }
  }
}
