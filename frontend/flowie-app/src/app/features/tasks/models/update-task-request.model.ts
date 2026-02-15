import { TaskStatus } from "./task-status.enum";

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  taskTypeId: number;
  dueDate?: string;
  employeeId?: number;
  status: TaskStatus;
}
