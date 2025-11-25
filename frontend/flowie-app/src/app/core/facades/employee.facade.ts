import { Injectable, inject, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { catchError, EMPTY, finalize } from "rxjs";
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
  providedIn: "root",
})
export class EmployeeFacade {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/api/employees`;

  #employees = signal<EmployeeDto[]>([]);
  #isLoading = signal<boolean>(false);

  employees = this.#employees.asReadonly();
  isLoading = this.#isLoading.asReadonly();

  getEmployees(): void {
    this.#isLoading.set(true);

    this.http
      .get<GetEmployeesResponse>(this.apiUrl)
      .pipe(
        catchError((error) => {
          console.error("Error loading employees:", error);
          this.#employees.set([]);
          return EMPTY;
        }),
        finalize(() => this.#isLoading.set(false))
      )
      .subscribe((response) => {
        this.#employees.set(response.employees);
      });
  }

  getEmployeeById(id: number): void {
    this.#isLoading.set(true);

    this.http
      .get<EmployeeDto>(`${this.apiUrl}/${id}`)
      .pipe(
        catchError((error) => {
          console.error("Error loading employee:", error);
          return EMPTY;
        }),
        finalize(() => this.#isLoading.set(false))
      )
      .subscribe((employee) => {
        // Update the employee in the list if it exists, or add it
        this.#employees.update((employees) => {
          const index = employees.findIndex((e) => e.id === id);
          if (index !== -1) {
            const updated = [...employees];
            updated[index] = employee;
            return updated;
          }
          return [...employees, employee];
        });
      });
  }
}
