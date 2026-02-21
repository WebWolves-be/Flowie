import { TaskStatus } from "./task-status.enum";

export interface Subtask {
  taskId: number;
  parentTaskId?: number | null;
  title: string;
  description?: string | null;
  taskTypeId: number;
  taskTypeName: string;
  dueDate?: string | null;
  status: TaskStatus;
  employeeId?: number | null;
  employeeName?: string | null;
  createdAt: string;
  updatedAt?: string | null;
  completedAt?: string | null;
  waitingSince?: string | null;
  displayOrder: number;
}
