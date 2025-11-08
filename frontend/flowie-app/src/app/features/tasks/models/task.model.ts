import { Subtask } from "./subtask.model";
import { TaskStatus } from "./task-status.enum";

export interface Task {
  id: number; // taskId backend
  projectId: number;
  parentTaskId?: number | null;
  title: string;
  description?: string | null;
  typeId?: number; // backend typeId
  typeName?: string; // backend typeName
  status?: TaskStatus; // backend status enum
  statusName?: string; // backend statusName string
  dueDate?: string | null; // ISO string (DateOnly)
  assignee: {
    id?: number | null; // employeeId backend
    name: string; // required in UI/back-end for now
    initials?: string; // UI convenience
    avatar?: string;
  };
  createdAt?: string; // ISO DateTimeOffset
  updatedAt?: string | null;
  completedAt?: string | null;
  subtaskCount?: number;
  completedSubtaskCount?: number;
  progress?: number; // UI derived: completedSubtaskCount/subtaskCount or other formula
  subtasks?: Subtask[];
}


