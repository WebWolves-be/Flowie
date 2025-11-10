import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';

export interface EmployeeDto {
  id: number;
  name: string;
  email?: string;
}

export interface GetEmployeesResponse {
  employees: EmployeeDto[];
}

@Injectable({
  providedIn: 'root'
})
export class EmployeeApiService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/employees`;

  // Note: Backend does not currently have an employees endpoint
  // This service is a placeholder for future implementation
  // For now, employee data comes from task assignees or auth endpoints
  
  getEmployees(): Observable<GetEmployeesResponse> {
    return this.http.get<GetEmployeesResponse>(this.apiUrl);
  }

  getEmployeeById(id: number): Observable<EmployeeDto> {
    return this.http.get<EmployeeDto>(`${this.apiUrl}/${id}`);
  }
}
