import { computed, inject, Injectable, signal } from "@angular/core";
import { TaskFacade } from "../../tasks/task.facade";
import { DashboardMetric } from "../models/dashboard-metric.model";
import { Project } from "../../tasks/models/project.model";

@Injectable({ providedIn: "root" })
export class DashboardFacade {
  private taskFacade = inject(TaskFacade);

  #isLoading = signal<boolean>(false);
  isLoading = this.#isLoading.asReadonly();

  projects = this.taskFacade.projects;

  load(): void {
    if (this.projects().length === 0) {
      this.#isLoading.set(true);
      this.taskFacade.getProjects();
      setTimeout(() => this.#isLoading.set(false), 350);
    }
  }

  private allProjects = computed<Project[]>(() => this.projects());

  totalProjects = computed(() => this.allProjects().length);
  totalTasks = computed(() =>
    this.allProjects().reduce((acc, p) => acc + p.taskCount, 0)
  );
  completedTasks = computed(() =>
    this.allProjects().reduce((acc, p) => acc + p.completedTaskCount, 0)
  );
  private previousCompletedTasks = computed(() =>
    Math.round(this.completedTasks() * 0.9)
  );
  overallProgressPct = computed(() => {
    const projects = this.allProjects();
    if (!projects.length) return 0;
    const sum = projects.reduce((acc, p) => {
      const progress = p.taskCount > 0 ? Math.round((p.completedTaskCount / p.taskCount) * 100) : 0;
      return acc + progress;
    }, 0);
    return Math.round(sum / projects.length);
  });
  private previousProgressPct = computed(() =>
    Math.max(0, this.overallProgressPct() - 5)
  );
  activeProjects = computed(
    () =>
      this.allProjects().filter((p) => {
        const progress = p.taskCount > 0 ? Math.round((p.completedTaskCount / p.taskCount) * 100) : 0;
        return progress > 0 && progress < 100;
      }).length
  );
  private previousActiveProjects = computed(() =>
    Math.max(0, this.activeProjects() - 1)
  );
  completedProjects = computed(
    () => this.allProjects().filter((p) => {
      const progress = p.taskCount > 0 ? Math.round((p.completedTaskCount / p.taskCount) * 100) : 0;
      return progress === 100;
    }).length
  );
  private previousCompletedProjects = computed(() =>
    Math.max(0, this.completedProjects() - 1)
  );
  notStartedProjects = computed(
    () => this.allProjects().filter((p) => {
      const progress = p.taskCount > 0 ? Math.round((p.completedTaskCount / p.taskCount) * 100) : 0;
      return progress === 0;
    }).length
  );
  private previousNotStartedProjects = computed(() =>
    this.notStartedProjects()
  );

  private progressStatus = computed<"good" | "warn" | "bad">(() => {
    const v = this.overallProgressPct();
    if (v >= 70) return "good";
    if (v >= 30) return "warn";
    return "bad";
  });

  metrics = computed<DashboardMetric[]>(() => [
    {
      key: "totalTasks",
      label: "Totaal aantal taken",
      value: this.totalTasks(),
      description: "Alle taken over alle projecten"
    },
    {
      key: "completedTasks",
      label: "Afgeronde taken",
      value: this.completedTasks(),
      description: "Taken gemarkeerd als voltooid",
      delta: this.completedTasks() - this.previousCompletedTasks(),
      trend:
        this.completedTasks() === this.previousCompletedTasks()
          ? "flat"
          : this.completedTasks() > this.previousCompletedTasks()
            ? "up"
            : "down"
    },
    {
      key: "overallProgress",
      label: "Gemiddelde voortgang",
      value: this.overallProgressPct() + "%",
      description: "Gemiddelde projectvoortgang",
      status: this.progressStatus(),
      delta: this.overallProgressPct() - this.previousProgressPct(),
      trend:
        this.overallProgressPct() === this.previousProgressPct()
          ? "flat"
          : this.overallProgressPct() > this.previousProgressPct()
            ? "up"
            : "down"
    },
    {
      key: "activeProjects",
      label: "Actieve projecten",
      value: this.activeProjects(),
      description: "Projecten in uitvoering",
      delta: this.activeProjects() - this.previousActiveProjects(),
      trend:
        this.activeProjects() === this.previousActiveProjects()
          ? "flat"
          : this.activeProjects() > this.previousActiveProjects()
            ? "up"
            : "down"
    },
    {
      key: "completedProjects",
      label: "Afgeronde projecten",
      value: this.completedProjects(),
      description: "Projecten 100% voltooid",
      delta: this.completedProjects() - this.previousCompletedProjects(),
      trend:
        this.completedProjects() === this.previousCompletedProjects()
          ? "flat"
          : this.completedProjects() > this.previousCompletedProjects()
            ? "up"
            : "down"
    },
    {
      key: "notStartedProjects",
      label: "Nog niet gestart",
      value: this.notStartedProjects(),
      description: "Projecten zonder voortgang",
      delta: this.notStartedProjects() - this.previousNotStartedProjects(),
      trend:
        this.notStartedProjects() === this.previousNotStartedProjects()
          ? "flat"
          : this.notStartedProjects() > this.previousNotStartedProjects()
            ? "up"
            : "down"
    }
  ]);
}
