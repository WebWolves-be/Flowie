import { Injectable, inject } from "@angular/core";
import { HttpClient, HttpParams } from "@angular/common/http";
import { Observable } from "rxjs";
import { environment } from "../../../environments/environment";
import { Company } from "../../features/tasks/models/company.enum";

export interface ProjectDto {
  projectId: number;
  title: string;
  company: Company;
  taskCount: number;
  completedTaskCount: number;
}

export interface GetProjectsResponse {
  projects: ProjectDto[];
}

export interface CreateProjectRequest {
  title: string;
  description?: string;
  company: Company;
}

export interface UpdateProjectRequest {
  projectId: number;
  title: string;
  description?: string;
  company: Company;
}

@Injectable({
  providedIn: "root",
})
export class ProjectApiService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/projects`;

  getProjects(company?: Company): Observable<GetProjectsResponse> {
    let params = new HttpParams();
    if (company) {
      params = params.set("company", company.toString());
    }
    return this.http.get<GetProjectsResponse>(this.apiUrl, { params });
  }

  getProjectById(id: number): Observable<ProjectDto> {
    return this.http.get<ProjectDto>(`${this.apiUrl}/${id}`);
  }

  createProject(request: CreateProjectRequest): Observable<void> {
    return this.http.post<void>(this.apiUrl, request);
  }

  updateProject(request: UpdateProjectRequest): Observable<void> {
    return this.http.put<void>(this.apiUrl, request);
  }
}
