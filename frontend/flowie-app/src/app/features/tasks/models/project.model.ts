
import { Company } from './company.enum';

export interface Project {
    id: number;
    name: string;
    taskCount: number;
    progress: number;
    company: Company;
}
