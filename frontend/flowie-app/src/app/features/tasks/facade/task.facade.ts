import { Injectable, signal, computed, inject } from "@angular/core";
import { Task } from "../models/task.model";
import { Project } from "../models/project.model";
import { Employee } from "../models/employee.model";
import { Company } from "../models/company.enum";
import { TaskStatus } from "../models/task-status.enum";
import { ProjectApiService } from "../../../core/services/project-api.service";
import { TaskApiService } from "../../../core/services/task-api.service";
import { EmployeeApiService } from "../../../core/services/employee-api.service";

@Injectable({
  providedIn: 'root'
})
export class TaskFacade {
  private projectApi = inject(ProjectApiService);
  private taskApi = inject(TaskApiService);
  private employeeApi = inject(EmployeeApiService);

  #projects = signal<Project[]>([]);
  #tasks = signal<Task[]>([]);
  #employees = signal<Employee[]>([]);
  #isLoadingProjects = signal<boolean>(false);
  #isLoadingTasks = signal<boolean>(false);
  #isLoadingEmployees = signal<boolean>(false);
  #companyFilter = signal<'ALL' | Company>('ALL');

  projects = this.#projects.asReadonly();
  tasks = this.#tasks.asReadonly();
  employees = this.#employees.asReadonly();
  isLoadingProjects = this.#isLoadingProjects.asReadonly();
  isLoadingTasks = this.#isLoadingTasks.asReadonly();
  isLoadingEmployees = this.#isLoadingEmployees.asReadonly();
  companyFilter = this.#companyFilter.asReadonly();

  getProjects(company?: Company): void {
    this.#isLoadingProjects.set(true);
    
    this.projectApi.getProjects(company).subscribe({
      next: (response) => {
        // Map backend DTOs to frontend Project model
        const projects: Project[] = response.projects.map(dto => ({
          id: dto.projectId,
          title: dto.title,
          description: undefined, // Backend doesn't return description in list
          taskCount: dto.taskCount,
          completedTaskCount: dto.completedTaskCount,
          progress: dto.taskCount > 0 
            ? Math.round((dto.completedTaskCount / dto.taskCount) * 100)
            : 0,
          company: dto.company
        }));
        
        this.#projects.set(projects);
        this.#isLoadingProjects.set(false);
      },
      error: (error) => {
        console.error('Error loading projects:', error);
        this.#projects.set([]);
        this.#isLoadingProjects.set(false);
      }
    });
  }

  getTasks(projectId: number, showOnlyMyTasks = false): void {
    this.#isLoadingTasks.set(true);
    
    this.taskApi.getTasks(projectId, showOnlyMyTasks).subscribe({
      next: (response) => {
        // Map backend DTOs to frontend Task model
        const tasks: Task[] = response.tasks.map(dto => ({
          id: dto.id,
          projectId: dto.projectId,
          title: dto.title,
          description: dto.description,
          typeId: dto.typeId,
          typeName: dto.typeName,
          status: dto.status,
          statusName: dto.statusName,
          dueDate: dto.dueDate,
          progress: dto.progress,
          assignee: dto.assignee,
          createdAt: dto.createdAt,
          updatedAt: dto.updatedAt,
          completedAt: dto.completedAt,
          subtasks: dto.subtasks,
          subtaskCount: dto.subtaskCount,
          completedSubtaskCount: dto.completedSubtaskCount
        }));
        
        this.#tasks.set(tasks);
        this.#isLoadingTasks.set(false);
      },
      error: (error) => {
        console.error('Error loading tasks:', error);
        this.#tasks.set([]);
        this.#isLoadingTasks.set(false);
      }
    });
  }

  clearTasks(): void {
    this.#tasks.set([]);
  }

  setCompanyFilter(filter: 'ALL' | Company): void {
    this.#companyFilter.set(filter);
    this.getProjects(filter === 'ALL' ? undefined : filter);
  }

  createProject(project: Omit<Project, 'id'>): void {
    // Call API to create project
    this.projectApi.createProject({
      title: project.title,
      description: project.description || undefined,
      company: project.company
    }).subscribe({
      next: () => {
        // Refresh project list after creation
        const filter = this.#companyFilter();
        this.getProjects(filter === 'ALL' ? undefined : filter);
      },
      error: (error) => {
        console.error('Error creating project:', error);
      }
    });
  }

  updateProject(updated: Project): void {
    // Call API to update project
    this.projectApi.updateProject({
      projectId: updated.id,
      title: updated.title,
      description: updated.description || undefined,
      company: updated.company
    }).subscribe({
      next: () => {
        // Update local state
        const current = this.#projects();
        const next = current.map(p => (p.id === updated.id ? { ...p, ...updated } : p));
        this.#projects.set(next);
      },
      error: (error) => {
        console.error('Error updating project:', error);
      }
    });
  }

  createTask(task: Omit<Task, 'id'>): void {
    // Call API to create task
    if (!task.typeId || !task.dueDate || !task.assignee.id) {
      console.error('Missing required task fields');
      return;
    }

    this.taskApi.createTask({
      projectId: task.projectId,
      title: task.title,
      taskTypeId: task.typeId,
      dueDate: task.dueDate,
      employeeId: task.assignee.id,
      description: task.description || undefined,
      parentTaskId: task.parentTaskId || undefined
    }).subscribe({
      next: () => {
        // Refresh task list after creation
        this.getTasks(task.projectId, false);
      },
      error: (error) => {
        console.error('Error creating task:', error);
      }
    });
  }

  updateTask(updated: Task): void {
    // Call API to update task
    if (!updated.typeId || !updated.dueDate || !updated.assignee.id || !updated.status) {
      console.error('Missing required task fields for update');
      return;
    }

    this.taskApi.updateTask({
      taskId: updated.id,
      title: updated.title,
      description: updated.description || undefined,
      typeId: updated.typeId,
      dueDate: updated.dueDate,
      assigneeId: updated.assignee.id,
      status: updated.status,
      progress: updated.progress || 0
    }).subscribe({
      next: () => {
        // Update local state
        const current = this.#tasks();
        const next = current.map(t => (t.id === updated.id ? { ...t, ...updated } : t));
        this.#tasks.set(next);
      },
      error: (error) => {
        console.error('Error updating task:', error);
      }
    });
  }

  getEmployees(): void {
    this.#isLoadingEmployees.set(true);
    
    // Note: Backend doesn't have employee endpoint yet, so this will fail
    // Consider keeping mock data or implementing backend endpoint
    this.employeeApi.getEmployees().subscribe({
      next: (response) => {
        const employees: Employee[] = response.employees.map(dto => ({
          id: dto.id,
          name: dto.name
        }));
        
        this.#employees.set(employees);
        this.#isLoadingEmployees.set(false);
      },
      error: (error) => {
        console.error('Error loading employees (endpoint may not exist):', error);
        // Fallback to mock data for now
        const mockEmployees: Employee[] = [
          { id: 1, name: 'Amalia Van Dosselaer' },
          { id: 2, name: 'Peter Carrein' },
          { id: 3, name: 'Jens Declerck' },
          { id: 4, name: 'Sophie Vermeulen' },
          { id: 5, name: 'Tom Janssens' },
          { id: 6, name: 'Lisa Peeters' },
          { id: 7, name: 'Marc De Vos' },
          { id: 8, name: 'Emma Claes' }
        ];
        this.#employees.set(mockEmployees);
        this.#isLoadingEmployees.set(false);
      }
    });
  }

}
