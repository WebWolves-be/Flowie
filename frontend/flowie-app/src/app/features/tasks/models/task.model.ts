import { Subtask } from "./subtask.model";

export interface Task {
  id: number;
  projectId: number;
  title: string;
  description: string;
  completed: boolean;
  category: string;
  deadline: string;
  assignee: {
    name: string;
    initials: string;
    avatar?: string;
  };
  progress?: number;
  subtasks?: Subtask[];
}


