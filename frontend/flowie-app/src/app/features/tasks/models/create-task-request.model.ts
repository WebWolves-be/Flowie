export interface CreateTaskRequest {
  sectionId: number;
  title: string;
  description?: string;
  taskTypeId: number;
  dueDate?: string;
  employeeId?: number;
  parentTaskId?: number;
}
