import { Task } from "./task.model";

export interface DeleteTaskDialogData {
  task: Task;
  isSubtask?: boolean;
}