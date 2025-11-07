import { Injectable, signal, computed } from "@angular/core";
import { Task } from "../models/task.model";
import { Project } from "../models/project.model";
import { Company } from "../models/company.enum";

@Injectable({
  providedIn: 'root'
})
export class TaskFacade {
  #projects = signal<Project[]>([]);
  #tasks = signal<Task[]>([]);
  #isLoadingProjects = signal<boolean>(false);
  #isLoadingTasks = signal<boolean>(false);

  projects = this.#projects.asReadonly();
  tasks = this.#tasks.asReadonly();
  isLoadingProjects = this.#isLoadingProjects.asReadonly();
  isLoadingTasks = this.#isLoadingTasks.asReadonly();

  getProjects(company?: Company): void {
    this.#isLoadingProjects.set(true);
    
    // Simulate API delay
    setTimeout(() => {
      const allMockProjects: Project[] = [
        {
          id: 1,
          name: 'Vijfwegstraat Gedan',
          taskCount: 4,
          completedTasks: 2,
          progress: 50,
          company: Company.Immoseed
        },
        {
          id: 2,
          name: 'Bruggestraat 31',
          taskCount: 7,
          completedTasks: 4,
          progress: 60,
          company: Company.NovaraRealEstate
        },
        {
          id: 3,
          name: 'Residentie Baku Gedan',
          taskCount: 3,
          completedTasks: 2,
          progress: 80,
          company: Company.NovaraRealEstate
        },
        // Fully completed example project
        {
          id: 4,
          name: 'Demo: Volledig Afgerond',
          taskCount: 3,
          completedTasks: 3,
          progress: 100,
          company: Company.Immoseed
        },
        // Zero progress example project
        {
          id: 5,
          name: 'Demo: Nog Niet Begonnen',
          taskCount: 5,
          completedTasks: 0,
          progress: 0,
          company: Company.NovaraRealEstate
        }
      ];
      const filtered = company ? allMockProjects.filter(p => p.company === company) : allMockProjects;
      
      this.#projects.set(filtered);
      this.#isLoadingProjects.set(false);
    }, 300);
  }

  getTasks(projectId: number, showOnlyMyTasks = false): void {
    this.#isLoadingTasks.set(true);
    
    // Simulate API delay
    setTimeout(() => {
      const mockTasks: Task[] = [
        // Project 1 tasks
        {
          id: 1,
          projectId: 1,
          title: 'Compromis maken',
          description: 'Voltooi de verkoopovereenkomst voor het Vijfwegstraat Gedan pand',
          completed: true,
          taskType: 'Compromis',
          deadline: '18 juni 2025',
          progress: 100,
          assignee: { name: 'Peter Carrein', initials: 'PC' }
        },
        {
          id: 2,
          projectId: 1,
          title: 'Nieuwe 3D tekeningen?',
          description: 'Maak nieuwe 3D architecturale tekeningen voor het project',
          completed: false,
          taskType: 'Ontwerp',
          deadline: '26 augustus 2025',
          progress: 0,
          assignee: { name: 'Peter Carrein', initials: 'PC' }
        },
        {
          id: 5,
          projectId: 1,
          title: 'Klantmeeting voorbereiden',
          description: 'Agenda opstellen en documenten verzamelen voor klantmeeting',
          completed: false,
          taskType: 'Communicatie',
          deadline: '15 december 2025',
          progress: 0,
          assignee: { name: 'Amalia Van Dosselaer', initials: 'AV' }
        },
        {
          id: 6,
          projectId: 1,
          title: 'Budgetcontrole Q2',
          description: 'Controleer de kosten en budgetten voor Q2',
          completed: false,
          taskType: 'Financiën',
          deadline: '31 december 2025',
          progress: 0,
          assignee: { name: 'Peter Carrein', initials: 'PC' }
        },
        // Project 2 tasks
        {
          id: 3,
          projectId: 2,
          title: 'Publiciteit ter plaatse',
          description: 'Opzetten van reclame en bewegwijzering op locatie',
          completed: true,
          taskType: 'Marketing',
          deadline: '18 juni 2025',
          progress: 100,
          assignee: { name: 'Peter Carrein', initials: 'PC' }
        },
        {
          id: 4,
          projectId: 2,
          title: 'Gewijzigde verkoopplannen L1 en L2 online zetten',
          description: 'Upload herziene verkoopplannen voor niveaus L1 en L2 naar het online platform',
          completed: false,
          taskType: 'Documentatie',
          deadline: '25 juni 2025',
          progress: 33,
          assignee: { name: 'Amalia Van Dosselaer', initials: 'AV' },
          subtasks: [
            { title: 'Huidige L1 plannen bekijken', assignee: 'Amalia Van Dosselaer', deadline: '20 juni 2025', done: true },
            { title: 'L2 verkoopplannen bijwerken', assignee: 'Amalia Van Dosselaer', deadline: '23 juni 2025', done: false },
            { title: 'Upload naar online platform', assignee: 'Amalia Van Dosselaer', deadline: '25 juni 2025', done: false }
          ]
        },
        {
          id: 7,
          projectId: 2,
          title: 'Contractreview aannemer',
          description: 'Controleer contractvoorwaarden en leveringsschema',
          completed: false,
          taskType: 'Juridisch',
          deadline: '02 juli 2025',
          progress: 0,
          assignee: { name: 'Amalia Van Dosselaer', initials: 'AV' }
        },
        {
          id: 8,
          projectId: 2,
          title: 'Website content bijwerken',
          description: 'Projectpagina updaten met nieuwe visuals en tekst',
          completed: false,
          taskType: 'Marketing',
          deadline: '20 december 2025',
          progress: 0,
          assignee: { name: 'Jens Declerck', initials: 'JD' }
        },
        // Project 4 fully completed
        {
          id: 9,
          projectId: 4,
          title: 'Oplevering documenten',
          description: 'Alle opleverdocumenten zijn ondertekend en gearchiveerd',
          completed: true,
          taskType: 'Documentatie',
          deadline: '10 juli 2025',
          progress: 100,
          assignee: { name: 'Amalia Van Dosselaer', initials: 'AV' }
        },
        {
          id: 10,
          projectId: 4,
          title: 'Eindinspectie afgerond',
          description: 'Laatste inspectieronde succesvol doorlopen',
          completed: true,
          taskType: 'Kwaliteit',
          deadline: '10 juli 2025',
          progress: 100,
          assignee: { name: 'Amalia Van Dosselaer', initials: 'AV' }
        },
        {
          id: 11,
          projectId: 4,
          title: 'Facturatie voltooid',
          description: 'Alle facturen zijn verstuurd en betaald',
          completed: true,
          taskType: 'Financiën',
          deadline: '10 juli 2025',
          progress: 100,
          assignee: { name: 'Amalia Van Dosselaer', initials: 'AV' }
        },
        // Project 5 zero progress tasks
        {
          id: 12,
          projectId: 5,
          title: 'Kick-off plannen',
          description: 'Kick-off meeting inplannen met alle stakeholders',
          completed: false,
          taskType: 'Planning',
          deadline: '15 juli 2025',
          progress: 0,
          assignee: { name: 'Amalia Van Dosselaer', initials: 'AV' }
        },
        {
          id: 13,
          projectId: 5,
          title: 'Scope definiëren',
          description: 'Projectscope en deliverables vastleggen',
          completed: false,
          taskType: 'Analyse',
          deadline: '16 juli 2025',
          progress: 0,
          assignee: { name: 'Peter Carrein', initials: 'PC' }
        },
        {
          id: 14,
          projectId: 5,
          title: 'Budget opstellen',
          description: 'Initiële budgettering voorleggen voor goedkeuring',
          completed: false,
          taskType: 'Financiën',
          deadline: '17 juli 2025',
          progress: 0,
          assignee: { name: 'Jens Declerck', initials: 'JD' }
        },
        {
          id: 15,
          projectId: 5,
          title: 'Risicoanalyse',
          description: 'Belangrijkste projectrisico’s in kaart brengen',
          completed: false,
          taskType: 'Risico',
          deadline: '18 juli 2025',
          progress: 0,
          assignee: { name: 'Amalia Van Dosselaer', initials: 'AV' }
        },
        {
          id: 16,
          projectId: 5,
          title: 'Planning opstellen',
          description: 'Hoog-over planning en milestones definiëren',
          completed: false,
          taskType: 'Planning',
          deadline: '19 juli 2025',
          progress: 0,
          assignee: { name: 'Peter Carrein', initials: 'PC' }
        }
      ];
      // Filter by requested project and current user preference
      const base = mockTasks.filter(t => t.projectId === projectId);
      const finalTasks = showOnlyMyTasks
        ? base.filter(t => t.assignee.name === 'Amalia Van Dosselaer')
        : base;
      this.#tasks.set(finalTasks);
      this.#isLoadingTasks.set(false);
    }, 300);
  }

  clearTasks(): void {
    this.#tasks.set([]);
  }

}