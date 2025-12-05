import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, throwError } from "rxjs";
import { NotificationService } from "../services/notification.service";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 400) {
        // Bad Request - Show validation errors from backend
        const errorMessage = extractErrorMessage(error);
        notificationService.showError(
          "Validatiefout",
          errorMessage,
          6000
        );
      } else if (error.status === 500) {
        // Server Error - Show generic error message
        notificationService.showError(
          "Er is iets misgegaan",
          "Probeer het later opnieuw of neem contact op met support.",
          5000
        );
      } else if (error.status === 0) {
        // Network error
        notificationService.showError(
          "Verbindingsprobleem",
          "Kan geen verbinding maken met de server.",
          5000
        );
      } else if (error.status === 404) {
        // Not Found
        notificationService.showError(
          "Niet gevonden",
          "De gevraagde resource bestaat niet.",
          4000
        );
      }

      // Log error for debugging
      console.error("HTTP Error:", error);

      return throwError(() => error);
    })
  );
};

function extractErrorMessage(error: HttpErrorResponse): string {
  if (error.error) {
    // Check for common error formats
    if (typeof error.error === "string") {
      return error.error;
    }
    
    if (error.error.message) {
      return error.error.message;
    }

    if (error.error.title) {
      return error.error.title;
    }

    if (error.error.errors) {
      // Handle validation errors (ASP.NET format)
      const errors = error.error.errors;
      const messages: string[] = [];
      
      for (const field in errors) {
        if (Array.isArray(errors[field])) {
          messages.push(...errors[field]);
        } else {
          messages.push(errors[field]);
        }
      }
      
      return messages.join(", ");
    }
  }

  return error.message || "Er is een fout opgetreden.";
}
