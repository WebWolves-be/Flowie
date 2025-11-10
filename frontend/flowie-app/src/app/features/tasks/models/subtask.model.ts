
import { TaskStatus } from './task-status.enum';

export interface Subtask {
    id?: number; // maps to taskId in SubtaskDto
    parentTaskId?: number | null;
    title: string;
    description?: string | null;
    dueDate?: string | null; // ISO DateOnly
    status?: TaskStatus;
    statusName?: string;
    assignee: {
        id?: number | null;
        name: string; // required (always assigned now)
    };
    createdAt?: string; // ISO DateTimeOffset
    updatedAt?: string | null;
    completedAt?: string | null;
    done?: boolean; // UI convenience (derived from status === Done)
}
