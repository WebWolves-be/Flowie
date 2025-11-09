import { Injectable, signal, computed } from "@angular/core";
import { Task } from "../models/task.model";
import { Project } from "../models/project.model";
import { Employee } from "../models/employee.model";
import { Company } from "../models/company.enum";
import { TaskStatus } from "../models/task-status.enum";

@Injectable({
  providedIn: 'root'
})
export class TaskFacade {
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
    
    // Simulate API delay
    setTimeout(() => {
      const allMockProjects: Project[] = [
        {
          id: 1,
          title: 'Vijfwegstraat Gedan',
          description: 'Renovatie van historisch pand in centrum van Gedan',
          taskCount: 4,
          completedTaskCount: 2,
          progress: 50,
          company: Company.Immoseed
        },
        {
          id: 2,
          title: 'Bruggestraat 31',
          description: 'Nieuwbouw appartementen met commerciële ruimte op de begane grond',
          taskCount: 7,
          completedTaskCount: 4,
          progress: 60,
          company: Company.NovaraRealEstate
        },
        {
          id: 3,
          title: 'Residentie Baku Gedan',
          description: 'Luxe appartementscomplex met 24 wooneenheden en ondergrondse parking',
          taskCount: 3,
          completedTaskCount: 2,
          progress: 80,
          company: Company.NovaraRealEstate
        },
        {
          id: 4,
          title: 'Demo: Volledig Afgerond',
          description: 'Test project om volledig afgeronde projecten te tonen',
          taskCount: 3,
          completedTaskCount: 3,
          progress: 100,
          company: Company.Immoseed
        },
        {
          id: 5,
          title: 'Demo: Nog Niet Begonnen',
          description: 'Test project in initiële fase zonder vooruitgang',
          taskCount: 5,
          completedTaskCount: 0,
          progress: 0,
          company: Company.NovaraRealEstate
        },
        {
          id: 6,
          title: 'Kerkstraat 45',
          description: 'Verbouwing van voormalige kerk tot woonruimtes',
          taskCount: 8,
          completedTaskCount: 3,
          progress: 37,
          company: Company.Immoseed
        },
        {
          id: 7,
          title: 'Stationsplein 12',
          description: 'Mixed-use development met retail en kantoorruimte',
          taskCount: 6,
          completedTaskCount: 5,
          progress: 83,
          company: Company.NovaraRealEstate
        },
        {
          id: 8,
          title: 'Residentie De Waterkant',
          description: 'Exclusieve penthouse appartementen met uitzicht op de rivier',
          taskCount: 12,
          completedTaskCount: 4,
          progress: 33,
          company: Company.Immoseed
        },
        {
          id: 9,
          title: 'Antwerpsesteenweg 89',
          description: 'Herbestemming van industrieel pand naar lofts',
          taskCount: 4,
          completedTaskCount: 1,
          progress: 25,
          company: Company.NovaraRealEstate
        },
        {
          id: 10,
          title: 'Leopoldlaan 23',
          description: 'Nieuwe residentie met duurzame energieoplossingen',
          taskCount: 9,
          completedTaskCount: 6,
          progress: 66,
          company: Company.Immoseed
        },
        {
          id: 11,
          title: 'Groenplaats Tower',
          description: 'Hoogbouwproject met 120 appartementen verdeeld over 18 verdiepingen',
          taskCount: 15,
          completedTaskCount: 8,
          progress: 53,
          company: Company.NovaraRealEstate
        },
        {
          id: 12,
          title: 'Meir 104-108',
          description: 'Restauratie van Art Deco gevels met moderne interieurinrichting',
          taskCount: 7,
          completedTaskCount: 2,
          progress: 28,
          company: Company.Immoseed
        },
        {
          id: 13,
          title: 'Residentie Park View',
          description: 'Groene woonwijk met gezamenlijke tuinen en speelruimte',
          taskCount: 10,
          completedTaskCount: 9,
          progress: 90,
          company: Company.NovaraRealEstate
        },
        {
          id: 14,
          title: 'Lange Nieuwstraat 67',
          description: 'Transformatie van kantoorgebouw naar studentenhuisvesting',
          taskCount: 5,
          completedTaskCount: 0,
          progress: 0,
          company: Company.Immoseed
        },
        {
          id: 15,
          title: 'Scheldekaaien Loft',
          description: 'Waterfront ontwikkeling met eigentijdse architectuur en terras',
          taskCount: 11,
          completedTaskCount: 5,
          progress: 45,
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
          typeId: 1,
          typeName: 'Compromis',
          status: TaskStatus.Done,
          statusName: 'Done',
          dueDate: '18 juni 2025',
          progress: 100,
          assignee: { name: 'Peter Carrein' },
          createdAt: new Date().toISOString(),
          completedAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 2,
          projectId: 1,
          title: 'Nieuwe 3D tekeningen?',
          description: 'Maak nieuwe 3D architecturale tekeningen voor het project',
          typeId: 2,
          typeName: 'Ontwerp',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '26 augustus 2025',
          progress: 0,
          assignee: { name: 'Peter Carrein' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 5,
          projectId: 1,
          title: 'Klantmeeting voorbereiden',
          description: 'Agenda opstellen en documenten verzamelen voor klantmeeting',
          typeId: 3,
          typeName: 'Communicatie',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '15 december 2025',
          progress: 0,
          assignee: { name: 'Amalia Van Dosselaer' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 6,
          projectId: 1,
          title: 'Budgetcontrole Q2',
          description: 'Controleer de kosten en budgetten voor Q2',
          typeId: 4,
          typeName: 'Financiën',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '31 december 2025',
          progress: 0,
          assignee: { name: 'Peter Carrein' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        // Project 2 tasks
        {
          id: 3,
          projectId: 2,
          title: 'Publiciteit ter plaatse',
          description: 'Opzetten van reclame en bewegwijzering op locatie',
          typeId: 5,
          typeName: 'Marketing',
          status: TaskStatus.Done,
          statusName: 'Done',
          dueDate: '18 juni 2025',
          progress: 100,
          assignee: { name: 'Peter Carrein' },
          createdAt: new Date().toISOString(),
          completedAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 4,
          projectId: 2,
          title: 'Gewijzigde verkoopplannen L1 en L2 online zetten',
          description: 'Upload herziene verkoopplannen voor niveaus L1 en L2 naar het online platform',
          typeId: 6,
          typeName: 'Documentatie',
          status: TaskStatus.Ongoing,
          statusName: 'Ongoing',
          dueDate: '25 juni 2025',
          progress: 33,
          assignee: { name: 'Amalia Van Dosselaer' },
          createdAt: new Date().toISOString(),
          subtasks: [
            { title: 'Huidige L1 plannen bekijken', assignee: { name: 'Amalia Van Dosselaer' }, dueDate: '20 juni 2025', done: true, status: TaskStatus.Done, statusName: 'Done' },
            { title: 'L2 verkoopplannen bijwerken', assignee: { name: 'Amalia Van Dosselaer' }, dueDate: '23 juni 2025', done: false, status: TaskStatus.Pending, statusName: 'Pending' },
            { title: 'Upload naar online platform', assignee: { name: 'Amalia Van Dosselaer' }, dueDate: '25 juni 2025', done: false, status: TaskStatus.Pending, statusName: 'Pending' }
          ],
          subtaskCount: 3,
          completedSubtaskCount: 1
        },
        {
          id: 7,
          projectId: 2,
          title: 'Contractreview aannemer',
          description: 'Controleer contractvoorwaarden en leveringsschema',
          typeId: 7,
          typeName: 'Juridisch',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '02 juli 2025',
          progress: 0,
          assignee: { name: 'Amalia Van Dosselaer' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 8,
          projectId: 2,
          title: 'Website content bijwerken',
          description: 'Projectpagina updaten met nieuwe visuals en tekst',
          typeId: 5,
          typeName: 'Marketing',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '20 december 2025',
          progress: 0,
          assignee: { name: 'Jens Declerck' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        // Project 4 fully completed
        {
          id: 9,
          projectId: 4,
          title: 'Oplevering documenten',
          description: 'Alle opleverdocumenten zijn ondertekend en gearchiveerd',
          typeId: 6,
          typeName: 'Documentatie',
          status: TaskStatus.Done,
          statusName: 'Done',
          dueDate: '10 juli 2025',
          progress: 100,
          assignee: { name: 'Amalia Van Dosselaer' },
          createdAt: new Date().toISOString(),
          completedAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 10,
          projectId: 4,
          title: 'Eindinspectie afgerond',
          description: 'Laatste inspectieronde succesvol doorlopen',
          typeId: 8,
          typeName: 'Kwaliteit',
          status: TaskStatus.Done,
          statusName: 'Done',
          dueDate: '10 juli 2025',
          progress: 100,
          assignee: { name: 'Amalia Van Dosselaer' },
          createdAt: new Date().toISOString(),
          completedAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 11,
          projectId: 4,
          title: 'Facturatie voltooid',
          description: 'Alle facturen zijn verstuurd en betaald',
          typeId: 4,
          typeName: 'Financiën',
          status: TaskStatus.Done,
          statusName: 'Done',
          dueDate: '10 juli 2025',
          progress: 100,
          assignee: { name: 'Amalia Van Dosselaer' },
          createdAt: new Date().toISOString(),
          completedAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        // Project 5 zero progress tasks
        {
          id: 12,
          projectId: 5,
          title: 'Kick-off plannen',
          description: 'Kick-off meeting inplannen met alle stakeholders',
          typeId: 9,
          typeName: 'Planning',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '15 juli 2025',
          progress: 0,
          assignee: { name: 'Amalia Van Dosselaer' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 13,
          projectId: 5,
          title: 'Scope definiëren',
          description: 'Projectscope en deliverables vastleggen',
          typeId: 10,
          typeName: 'Analyse',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '16 juli 2025',
          progress: 0,
          assignee: { name: 'Peter Carrein' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 14,
          projectId: 5,
          title: 'Budget opstellen',
          description: 'Initiële budgettering voorleggen voor goedkeuring',
          typeId: 4,
          typeName: 'Financiën',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '17 juli 2025',
          progress: 0,
          assignee: { name: 'Jens Declerck' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 15,
          projectId: 5,
          title: 'Risicoanalyse',
          description: 'Belangrijkste projectrisico’s in kaart brengen',
          typeId: 11,
          typeName: 'Risico',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '18 juli 2025',
          progress: 0,
          assignee: { name: 'Amalia Van Dosselaer' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
        },
        {
          id: 16,
          projectId: 5,
          title: 'Planning opstellen',
          description: 'Hoog-over planning en milestones definiëren',
          typeId: 9,
          typeName: 'Planning',
          status: TaskStatus.Pending,
          statusName: 'Pending',
          dueDate: '19 juli 2025',
          progress: 0,
          assignee: { name: 'Peter Carrein' },
          createdAt: new Date().toISOString(),
          subtaskCount: 0,
          completedSubtaskCount: 0
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

  setCompanyFilter(filter: 'ALL' | Company): void {
    this.#companyFilter.set(filter);
    this.getProjects(filter === 'ALL' ? undefined : filter);
  }

  createProject(project: Omit<Project, 'id'>): void {
    const current = this.#projects();
    const nextId = current.length ? Math.max(...current.map(p => p.id)) + 1 : 1;
    const normalized: Project = {
      ...project,
      id: nextId,
      taskCount: project.taskCount ?? 0,
      completedTaskCount: (project as any).completedTaskCount ?? 0,
      progress: typeof project.progress === 'number' ? project.progress : 0
    } as Project;
    this.#projects.set([...current, normalized]);
  }

  updateProject(updated: Project): void {
    const current = this.#projects();
    const next = current.map(p => (p.id === updated.id ? { ...p, ...updated } : p));
    this.#projects.set(next);
  }

  createTask(task: Omit<Task, 'id'>): void {
    const current = this.#tasks();
    const nextId = current.length ? Math.max(...current.map(t => t.id)) + 1 : 1;
    const normalized: Task = {
      ...task,
      id: nextId,
      status: task.status ?? TaskStatus.Pending,
      statusName: task.statusName ?? (task.status === TaskStatus.Done ? 'Done' : task.status === TaskStatus.Ongoing ? 'Ongoing' : 'Pending'),
      progress: typeof task.progress === 'number' ? task.progress : (task.status === TaskStatus.Done ? 100 : task.status === TaskStatus.Ongoing ? 33 : 0),
      createdAt: task.createdAt ?? new Date().toISOString(),
      updatedAt: new Date().toISOString(),
      completedAt: task.status === TaskStatus.Done ? (task.completedAt ?? new Date().toISOString()) : null,
      subtaskCount: task.subtaskCount ?? 0,
      completedSubtaskCount: task.completedSubtaskCount ?? 0
    } as Task;
    this.#tasks.set([...current, normalized]);
  }

  updateTask(updated: Task): void {
    const current = this.#tasks();
    const next = current.map(t => (t.id === updated.id ? { ...t, ...updated } : t));
    this.#tasks.set(next);
  }

  getEmployees(): void {
    this.#isLoadingEmployees.set(true);
    
    // Simulate API delay
    setTimeout(() => {
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
    }, 300);
  }

}