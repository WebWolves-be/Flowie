import { TaskStatus } from "./task-status.enum";
import { Subtask } from "./subtask.model";

export interface Task {
  taskId: number;
  projectId: number;
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
  subtaskCount: number;
  completedSubtaskCount: number;
  subtasks: Subtask[];
  displayOrder: number;
}
