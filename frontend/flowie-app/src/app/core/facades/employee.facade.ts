import { inject, Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../../environments/environment";

export interface EmployeeDto {
  id: number;
  name: string;
  email?: string;
}

export interface GetEmployeesResponse {
  employees: EmployeeDto[];
}

@Injectable({
  providedIn: "root"
})
export class EmployeeFacade {
  #http = inject(HttpClient);
  #apiUrl = `${environment.apiUrl}/api/employees`;

  #employees = signal<EmployeeDto[]>([]);

  employees = this.#employees.asReadonly();

  getEmployees(): void {
    this.#http
      .get<GetEmployeesResponse>(this.#apiUrl)
      .subscribe((response) => {
        this.#employees.set(response.employees);
      });
  }
}
