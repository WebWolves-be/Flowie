import { TaskStatus } from "./task-status.enum";

export interface Subtask {
  taskId: number;
  parentTaskId?: number | null;
  title: string;
  description?: string | null;
  typeId: number;
  typeName: string;
  dueDate: string;
  status: TaskStatus;
  employeeId: number;
  employeeName: string;
  createdAt: string;
  updatedAt?: string | null;
  completedAt?: string | null;
}