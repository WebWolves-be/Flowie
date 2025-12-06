import { TaskStatus } from "./task-status.enum";

export interface UpdateTaskRequest {
  title: string;
  description?: string;
  typeId: number;
  dueDate: string;
  employeeId: number;
  status: TaskStatus;
  progress: number;
}
