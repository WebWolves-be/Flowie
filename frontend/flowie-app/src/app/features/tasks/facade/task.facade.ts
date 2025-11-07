import { Injectable, signal, computed } from "@angular/core";
import { Task } from "../models/task.model";
import { Project } from "../models/project.model";
import { Company } from "../models/company.enum";

@Injectable({
  providedIn: 'root'
})
export class TaskFacade {
  // Private signals for internal state management
  #projects = signal<Project[]>([]);
  #tasks = signal<Task[]>([]);
  #selectedProjectId = signal<number | null>(null);
  #showOnlyMyTasks = signal<boolean>(false);
  #isLoadingProjects = signal<boolean>(false);
  #isLoadingTasks = signal<boolean>(false);

  // Public readonly signals
  projects = this.#projects.asReadonly();
  tasks = this.#tasks.asReadonly();
  selectedProjectId = this.#selectedProjectId.asReadonly();
  showOnlyMyTasks = this.#showOnlyMyTasks.asReadonly();
  isLoadingProjects = this.#isLoadingProjects.asReadonly();
  isLoadingTasks = this.#isLoadingTasks.asReadonly();

  // Computed signals
  selectedProject = computed(() => {
    return this.#projects().find(p => p.id === this.#selectedProjectId());
  });

  filteredTasks = computed(() => {
    const tasks = this.#tasks();
    const showOnlyMine = this.#showOnlyMyTasks();
    
    // For now, return all tasks. You can implement filtering logic here
    if (showOnlyMine) {
      return tasks.filter(t => t.assignee.name === 'Amalia Van Dosselaer');
    }
    return tasks;
  });

  constructor() {
    // Intentionally left empty; caller (TasksPage) will trigger initial load
  }

  // Fake API call to load projects
  loadProjects(company?: Company): void {
    this.#isLoadingProjects.set(true);
    
    // Simulate API delay
    setTimeout(() => {
      const allMockProjects: Project[] = [
        {
          id: 1,
          name: 'Vijfwegstraat Gedan',
          taskCount: 4,
          progress: 50,
          company: Company.Immoseed
        },
        {
          id: 2,
          name: 'Bruggestraat 31',
          taskCount: 7,
          progress: 60,
          company: Company.NovaraRealEstate
        },
        {
          id: 3,
          name: 'Residentie Baku Gedan',
          taskCount: 3,
          progress: 80,
          company: Company.NovaraRealEstate
        }
      ];
      const filtered = company ? allMockProjects.filter(p => p.company === company) : allMockProjects;
      
      this.#projects.set(filtered);
      this.#isLoadingProjects.set(false);
    }, 300);
  }

  // Fake API call to load tasks for a project
  loadTasks(projectId: number): void {
    this.#isLoadingTasks.set(true);
    
    // Simulate API delay
    setTimeout(() => {
      const mockTasks: Task[] = [
        {
          id: 1,
          projectId: 1,
          title: 'Compromis maken',
          description: 'Voltooi de verkoopovereenkomst voor het Vijfwegstraat Gedan pand',
          completed: true,
          category: 'Compromis',
          deadline: '18 juni 2025',
          progress: 100,
          assignee: {
            name: 'Peter Carrein',
            initials: 'PC'
          }
        },
        {
          id: 2,
          projectId: 1,
          title: 'Nieuwe 3D tekeningen?',
          description: 'Maak nieuwe 3D architecturale tekeningen voor het project',
          completed: false,
          category: 'Ontwerp',
          deadline: '26 augustus 2025',
          progress: 0,
          assignee: {
            name: 'Peter Carrein',
            initials: 'PC'
          }
        },
        {
          id: 3,
          projectId: 2,
          title: 'Publiciteit ter plaatse',
          description: 'Opzetten van reclame en bewegwijzering op locatie',
          completed: true,
          category: 'Marketing',
          deadline: '18 juni 2025',
          progress: 100,
          assignee: {
            name: 'Peter Carrein',
            initials: 'PC'
          }
        },
        {
          id: 4,
          projectId: 2,
          title: 'Gewijzigde verkoopplannen L1 en L2 online zetten',
          description: 'Upload herziene verkoopplannen voor niveaus L1 en L2 naar het online platform',
          completed: false,
          category: 'Documentatie',
          deadline: '25 juni 2025',
          progress: 33,
          assignee: {
            name: 'Amalia Van Dosselaer',
            initials: 'AV'
          },
          subtasks: [
            { title: 'Huidige L1 plannen bekijken', assignee: 'Amalia Van Dosselaer', deadline: '20 juni 2025', done: true },
            { title: 'L2 verkoopplannen bijwerken', assignee: 'Amalia Van Dosselaer', deadline: '23 juni 2025', done: false },
            { title: 'Upload naar online platform', assignee: 'Amalia Van Dosselaer', deadline: '25 juni 2025', done: false }
          ]
        }
      ];
      // Filter by requested project
      this.#tasks.set(mockTasks.filter(t => t.projectId === projectId));
      this.#isLoadingTasks.set(false);
    }, 300);
  }

  clearTasks(): void {
    this.#tasks.set([]);
  }

  // Action methods
  selectProject(projectId: number): void {
    this.#selectedProjectId.set(projectId);
  }

  clearProjectSelection(): void {
    this.#selectedProjectId.set(null);
  }

  toggleTask(taskId: number): void {
    this.#tasks.update(tasks => {
      const task = tasks.find(t => t.id === taskId);
      if (task) {
        task.completed = !task.completed;
        task.progress = task.completed ? 100 : 0;
      }
      return [...tasks];
    });
  }

  updateTask(taskId: number, updates: Partial<Task>): void {
    this.#tasks.update(tasks => {
      const taskIndex = tasks.findIndex(t => t.id === taskId);
      if (taskIndex !== -1) {
        tasks[taskIndex] = { ...tasks[taskIndex], ...updates };
      }
      return [...tasks];
    });
  }

  toggleSubtask(taskId: number, subtaskIndex: number): void {
    this.#tasks.update(tasks => {
      const task = tasks.find(t => t.id === taskId);
      if (task?.subtasks && task.subtasks[subtaskIndex]) {
        task.subtasks[subtaskIndex].done = !task.subtasks[subtaskIndex].done;
        
        // Update task progress based on subtasks
        const done = task.subtasks.filter(s => s.done).length;
        task.progress = Math.round((done / task.subtasks.length) * 100);
        task.completed = task.progress === 100;
      }
      return [...tasks];
    });
  }

  setShowOnlyMyTasks(value: boolean): void {
    this.#showOnlyMyTasks.set(value);
  }

  deleteTask(taskId: number): void {
    // Simulate API call
    this.#isLoadingTasks.set(true);
    setTimeout(() => {
      this.#tasks.update(tasks => tasks.filter(t => t.id !== taskId));
      this.#isLoadingTasks.set(false);
    }, 300);
  }
}