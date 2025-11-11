import { Company } from "./company.enum";

export interface Project {
  projectId: number;
  title: string;
  description: string;
  company: Company;
  taskCount: number;
  completedTaskCount: number;
}

