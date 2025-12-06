import { HttpErrorResponse } from "@angular/common/http";
import { ValidationError } from "../models/validation-error.model";

/**
 * Extracts a user-friendly error message from an HTTP error response.
 * Handles validation errors (400) with custom error messages and falls back to generic messages.
 *
 * @param error - The HTTP error response
 * @returns A formatted error message string
 */
export function extractErrorMessage(error: HttpErrorResponse): string {
  if (error.status === 400 && error.error?.errors) {
    const errors = error.error.errors as ValidationError[];
    if (Array.isArray(errors) && errors.length > 0) {
      return errors.map((e) => e.errorMessage).join(" ");
    }
  }
  if (error.error?.title) {
    return error.error.title;
  }
  return "Er is een fout opgetreden. Probeer het opnieuw.";
}
