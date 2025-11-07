import { Injectable, computed, signal, inject } from '@angular/core';
import { TaskFacade } from '../../tasks/facade/task.facade';
import { Project } from '../../tasks/models/project.model';
import { Task } from '../../tasks/models/task.model';
import { DashboardMetric } from '../models/dashboard-metric.model';

@Injectable({ providedIn: 'root' })
export class DashboardFacade {
  private taskFacade = inject(TaskFacade);

  // Local loading state for initial dashboard metrics (reuse project loading)
  #isLoading = signal<boolean>(false);
  isLoading = this.#isLoading.asReadonly();

  // Expose underlying data (projects) in case dashboard needs them
  projects = this.taskFacade.projects;

  // Trigger data load (projects are sufficient for now)
  load(): void {
    if (this.projects().length === 0) {
      this.#isLoading.set(true);
      this.taskFacade.getProjects();
      // Simulate completion when projects finish loading
      setTimeout(() => this.#isLoading.set(false), 350);
    }
  }

  // Helper to aggregate tasks across all projects by requesting tasks per project sequentially (MVP mock)
  // For now we approximate with project counts only (task list loading per project would add latency).
  private allProjects = computed<Project[]>(() => this.projects());

  // Metrics based purely on project aggregates (MVP) since we lack started/done timestamps.
  totalProjects = computed(() => this.allProjects().length);
  totalTasks = computed(() => this.allProjects().reduce((acc, p) => acc + p.taskCount, 0));
  completedTasks = computed(() => this.allProjects().reduce((acc, p) => acc + p.completedTasks, 0));
  // For mock delta we simulate a previous period as 90% of current values
  private previousCompletedTasks = computed(() => Math.round(this.completedTasks() * 0.9));
  overallProgressPct = computed(() => {
    const projects = this.allProjects();
    if (!projects.length) return 0;
    // Average project progress
    const sum = projects.reduce((acc, p) => acc + p.progress, 0);
    return Math.round(sum / projects.length);
  });
  private previousProgressPct = computed(() => Math.max(0, this.overallProgressPct() - 5));
  activeProjects = computed(() => this.allProjects().filter(p => p.progress > 0 && p.progress < 100).length);
  private previousActiveProjects = computed(() => Math.max(0, this.activeProjects() - 1));
  completedProjects = computed(() => this.allProjects().filter(p => p.progress === 100).length);
  private previousCompletedProjects = computed(() => Math.max(0, this.completedProjects() - 1));
  notStartedProjects = computed(() => this.allProjects().filter(p => p.progress === 0).length);
  private previousNotStartedProjects = computed(() => this.notStartedProjects());

  private progressStatus = computed<'good' | 'warn' | 'bad'>(() => {
    const v = this.overallProgressPct();
    if (v >= 70) return 'good';
    if (v >= 30) return 'warn';
    return 'bad';
  });

  metrics = computed<DashboardMetric[]>(() => [
    {
      key: 'totalTasks',
      label: 'Totaal aantal taken',
      value: this.totalTasks(),
      description: 'Alle taken over alle projecten'
    },
    {
      key: 'completedTasks',
      label: 'Afgeronde taken',
      value: this.completedTasks(),
      description: 'Taken gemarkeerd als voltooid',
      delta: this.completedTasks() - this.previousCompletedTasks(),
      trend: this.completedTasks() === this.previousCompletedTasks() ? 'flat' : (this.completedTasks() > this.previousCompletedTasks() ? 'up' : 'down')
    },
    {
      key: 'overallProgress',
      label: 'Gemiddelde voortgang',
      value: this.overallProgressPct() + '%',
      description: 'Gemiddelde projectvoortgang',
      status: this.progressStatus(),
      delta: this.overallProgressPct() - this.previousProgressPct(),
      trend: this.overallProgressPct() === this.previousProgressPct() ? 'flat' : (this.overallProgressPct() > this.previousProgressPct() ? 'up' : 'down')
    },
    {
      key: 'activeProjects',
      label: 'Actieve projecten',
      value: this.activeProjects(),
      description: 'Projecten in uitvoering',
      delta: this.activeProjects() - this.previousActiveProjects(),
      trend: this.activeProjects() === this.previousActiveProjects() ? 'flat' : (this.activeProjects() > this.previousActiveProjects() ? 'up' : 'down')
    },
    {
      key: 'completedProjects',
      label: 'Afgeronde projecten',
      value: this.completedProjects(),
      description: 'Projecten 100% voltooid',
      delta: this.completedProjects() - this.previousCompletedProjects(),
      trend: this.completedProjects() === this.previousCompletedProjects() ? 'flat' : (this.completedProjects() > this.previousCompletedProjects() ? 'up' : 'down')
    },
    {
      key: 'notStartedProjects',
      label: 'Nog niet gestart',
      value: this.notStartedProjects(),
      description: 'Projecten zonder voortgang',
      delta: this.notStartedProjects() - this.previousNotStartedProjects(),
      trend: this.notStartedProjects() === this.previousNotStartedProjects() ? 'flat' : (this.notStartedProjects() > this.previousNotStartedProjects() ? 'up' : 'down')
    }
  ]);
}
