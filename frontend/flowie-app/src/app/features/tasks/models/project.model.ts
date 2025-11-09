
import { Company } from './company.enum';

export interface Project {
    id: number;
    title: string; // aligns with backend Title
    description?: string | null;
    taskCount: number;
    completedTaskCount: number; // aligns with backend CompletedTaskCount
    progress: number; // derived client-side
    company: Company; // backend sends string; mapped to enum in UI
}
