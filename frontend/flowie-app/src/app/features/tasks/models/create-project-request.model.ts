import { Company } from "./company.enum";

export interface CreateProjectRequest {
  title: string;
  description?: string;
  company: Company;
}
