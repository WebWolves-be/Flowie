
import { Company } from './company.enum';

export interface Project {
    id: number;
    name: string;
    taskCount: number;
    completedTasks: number;
    progress: number;
    company: Company;
}
