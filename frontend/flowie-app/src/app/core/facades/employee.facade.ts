import { inject, Injectable, signal } from "@angular/core";
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
  providedIn: "root"
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
}
