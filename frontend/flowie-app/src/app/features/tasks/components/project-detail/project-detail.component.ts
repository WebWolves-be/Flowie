import { Component, computed, input, OnChanges, output, signal } from "@angular/core";
import { Company } from "../../models/company.enum";
import { TaskItemComponent } from "../task-item/task-item.component";
import { Project } from "../../models/project.model";
import { Section } from "../../models/section.model";
import { Task } from "../../models/task.model";
import { TaskStatus } from "../../models/task-status.enum";
import { CdkDragDrop, CdkDropList, CdkDrag, CdkDragHandle, moveItemInArray } from "@angular/cdk/drag-drop";

@Component({
  selector: "app-project-detail",
  standalone: true,
  imports: [TaskItemComponent, CdkDropList, CdkDrag, CdkDragHandle],
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
  orderedSections = computed(() => {
    return [...this.sections()].sort((a, b) => a.displayOrder - b.displayOrder);
  });

  getTasksForSection(sectionId: number): Task[] {
    return this.tasks().filter(t => t.sectionId === sectionId)
      .sort((a, b) => a.displayOrder - b.displayOrder);
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

  onSectionDrop(event: CdkDragDrop<Section[]>) {
    const reorderedSections = [...this.orderedSections()];
    moveItemInArray(reorderedSections, event.previousIndex, event.currentIndex);
    this.sectionReorderRequested.emit(reorderedSections.map((s, i) => ({ sectionId: s.sectionId, displayOrder: i })));
  }

  onTaskDrop(event: CdkDragDrop<Task[]>, sectionId: number) {
    const sectionTasks = this.getTasksForSection(sectionId);
    const reorderedTasks = [...sectionTasks];
    moveItemInArray(reorderedTasks, event.previousIndex, event.currentIndex);
    this.taskReorderRequested.emit(reorderedTasks.map((t, i) => ({ taskId: t.taskId, displayOrder: i })));
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
