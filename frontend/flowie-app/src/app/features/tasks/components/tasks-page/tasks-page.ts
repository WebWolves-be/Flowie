import { Component, inject, effect, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, ActivatedRoute } from '@angular/router';
import { ProjectListComponent } from '../project-list/project-list.component';
import { TaskItemComponent } from '../task-item/task-item.component';
import { TaskFacade } from '../../facade/task.facade';
import { Company } from '../../models/company.enum';

@Component({
  selector: 'app-tasks-page',
  standalone: true,
  imports: [CommonModule, ProjectListComponent, TaskItemComponent],
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
  tasks = this.facade.filteredTasks;
  selectedProject = this.facade.selectedProject;
  selectedProjectId = this.facade.selectedProjectId;
  showOnlyMyTasks = this.facade.showOnlyMyTasks;
  isLoadingProjects = this.facade.isLoadingProjects;
  isLoadingTasks = this.facade.isLoadingTasks;

  // Expose enum to template
  readonly Company = Company;

  // Local filter state for projects list
  companyFilter = signal<'ALL' | Company>('ALL');

  constructor() {
    // Initial load of projects only if not already loaded (avoid flicker on first click)
    if (this.projects().length === 0) {
      this.facade.loadProjects();
    }

    // Subscribe to route params and update selected project + load tasks
    this.route.paramMap.subscribe(params => {
      const projectId = params.get('id');
      if (projectId) {
        const idNum = Number(projectId);
        this.facade.selectProject(idNum);
        this.facade.loadTasks(idNum);
      } else {
        this.facade.clearProjectSelection();
        this.facade.clearTasks();
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

  onTaskToggled(taskId: number) {
    this.facade.toggleTask(taskId);
  }

  onTaskClicked(taskId: number) {
    console.log('Task clicked:', taskId);
    // Implement task detail view or edit functionality
  }

  toggleTaskFilter(showOnlyMine: boolean) {
    this.facade.setShowOnlyMyTasks(showOnlyMine);
  }

  filterCompany(filter: 'ALL' | Company) {
    this.companyFilter.set(filter);
    // Clear current project selection and route when changing company filter
    this.facade.clearProjectSelection();
    this.router.navigate(['/taken']).catch(() => {});
    if (filter === 'ALL') {
      this.facade.loadProjects();
    } else {
      this.facade.loadProjects(filter);
    }
  }
}
