import { Component, effect, input, output, signal } from "@angular/core";
import { Company } from "../../models/company.enum";
import { TaskItemComponent } from "../task-item/task-item.component";
import { Project } from "../../models/project.model";
import { Section } from "../../models/section.model";
import { Task } from "../../models/task.model";
import { TaskStatus } from "../../models/task-status.enum";
import { CdkDragDrop, CdkDropList, CdkDrag, CdkDragHandle, moveItemInArray } from "@angular/cdk/drag-drop";
import { CdkScrollable } from "@angular/cdk/scrolling";

@Component({
  selector: "app-project-detail",
  standalone: true,
  imports: [TaskItemComponent, CdkDropList, CdkDrag, CdkDragHandle, CdkScrollable],
  templateUrl: "./project-detail.component.html",
  styleUrl: "./project-detail.component.scss"
})
export class ProjectDetailComponent {
  readonly Company = Company;

  project = input.required<Project>();
  sections = input<Section[]>([]);
  isLoadingSections = input<boolean>(false);
  tasks = input<Task[]>([]);
  isLoadingTasks = input<boolean>(false);
  isDetailLoading = input<boolean>(false);
  showOnlyMyTasks = input<boolean>(false);
  isMobile = input<boolean>(false);

  backToList = output<void>();
  taskFilterToggled = output<boolean>();
  projectUpdateRequested = output<void>();
  sectionCreateRequested = output<void>();
  sectionUpdateRequested = output<number>();
  sectionDeleteRequested = output<number>();
  taskCreateRequested = output<number>();
  taskUpdateRequested = output<number>();
  taskDeleteRequested = output<number>();
  taskStatusChanged = output<{ taskId: number; status: TaskStatus }>();
  subtaskCreateRequested = output<number>();
  subtaskUpdateRequested = output<number>();
  subtaskDeleteRequested = output<number>();
  subtaskStatusChanged = output<{ taskId: number; status: TaskStatus }>();
  taskReorderRequested = output<{ taskId: number; displayOrder: number }[]>();
  subtaskReorderRequested = output<{ taskId: number; displayOrder: number }[]>();
  sectionReorderRequested = output<{ sectionId: number; displayOrder: number }[]>();

  expandedSections = signal<Set<number>>(new Set());
  showSectionMenu = signal<number | null>(null);
  showProjectMenu = signal<boolean>(false);
  orderedSections = signal<Section[]>([]);
  #orderedTasksBySectionId = signal<Map<number, Task[]>>(new Map());

  constructor() {
    effect(() => {
      this.orderedSections.set([...this.sections()].sort((a, b) => a.displayOrder - b.displayOrder));
    });
    effect(() => {
      const map = new Map<number, Task[]>();
      for (const section of this.sections()) {
        map.set(
          section.sectionId,
          this.tasks().filter(t => t.sectionId === section.sectionId)
            .sort((a, b) => a.displayOrder - b.displayOrder)
        );
      }
      this.#orderedTasksBySectionId.set(map);
    });
  }

  toggleProjectMenu(event: Event): void {
    event.stopPropagation();
    this.showProjectMenu.update(v => !v);
  }

  getTasksForSection(sectionId: number): Task[] {
    return this.#orderedTasksBySectionId().get(sectionId) ?? [];
  }

  toggleSection(sectionId: number): void {
    const expanded = new Set(this.expandedSections());
    if (expanded.has(sectionId)) {
      expanded.delete(sectionId);
    } else {
      expanded.add(sectionId);
    }
    this.expandedSections.set(expanded);
  }

  toggleSectionMenu(event: Event, sectionId: number): void {
    event.stopPropagation();
    this.showSectionMenu.update(current => current === sectionId ? null : sectionId);
  }

  onSectionDrop(event: CdkDragDrop<Section[]>) {
    const reordered = [...this.orderedSections()];
    moveItemInArray(reordered, event.previousIndex, event.currentIndex);
    this.orderedSections.set(reordered);
    this.sectionReorderRequested.emit(reordered.map((s, i) => ({ sectionId: s.sectionId, displayOrder: i })));
  }

  onTaskDrop(event: CdkDragDrop<Task[]>, sectionId: number) {
    const reordered = [...this.getTasksForSection(sectionId)];
    moveItemInArray(reordered, event.previousIndex, event.currentIndex);
    const newMap = new Map(this.#orderedTasksBySectionId());
    newMap.set(sectionId, reordered);
    this.#orderedTasksBySectionId.set(newMap);
    this.taskReorderRequested.emit(reordered.map((t, i) => ({ taskId: t.taskId, displayOrder: i })));
  }

  onToggleTaskFilter(val: boolean) {
    this.taskFilterToggled.emit(val);
  }

  onUpdateProject() {
    this.projectUpdateRequested.emit();
  }

  onCreateSection() {
    this.sectionCreateRequested.emit();
  }

  onUpdateSection(sectionId: number) {
    this.sectionUpdateRequested.emit(sectionId);
  }

  onDeleteSection(sectionId: number) {
    this.sectionDeleteRequested.emit(sectionId);
  }

  onCreateTaskInSection(event: Event, sectionId: number) {
    event.stopPropagation();
    this.taskCreateRequested.emit(sectionId);
  }

  onTaskUpdate(id: number) {
    this.taskUpdateRequested.emit(id);
  }

  onTaskDelete(id: number) {
    this.taskDeleteRequested.emit(id);
  }

  onTaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    this.taskStatusChanged.emit(event);
  }

  onSubtaskCreate(taskId: number) {
    this.subtaskCreateRequested.emit(taskId);
  }

  onSubtaskUpdate(subtaskId: number) {
    this.subtaskUpdateRequested.emit(subtaskId);
  }

  onSubtaskDelete(subtaskId: number) {
    this.subtaskDeleteRequested.emit(subtaskId);
  }

  onSubtaskStatusChanged(event: { taskId: number; status: TaskStatus }) {
    this.subtaskStatusChanged.emit(event);
  }

  onSubtaskReorder(items: { taskId: number; displayOrder: number }[]) {
    this.subtaskReorderRequested.emit(items);
  }
}
