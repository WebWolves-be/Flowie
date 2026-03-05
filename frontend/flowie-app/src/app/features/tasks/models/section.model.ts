export interface Section {
  sectionId: number;
  projectId: number;
  title: string;
  description: string | null;
  displayOrder: number;
  taskCount: number;
  completedTaskCount: number;
}
