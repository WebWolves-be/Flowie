import { TaskStatus } from "./task-status.enum";
import { Subtask } from "./subtask.model";

export interface Task {
  taskId: number;
  projectId: number;
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
  subtaskCount: number;
  completedSubtaskCount: number;
  subtasks: Subtask[];
}
