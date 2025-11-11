import { Company } from "./company.enum";

export interface UpdateProjectRequest {
  title: string;
  description?: string;
  company: Company;
}
