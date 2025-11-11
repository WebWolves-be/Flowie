import { TaskStatus } from "./task-status.enum";

export interface UpdateTaskStatusRequest {
  status: TaskStatus;
}
