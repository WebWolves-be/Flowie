import { HttpErrorResponse, HttpInterceptorFn } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, throwError } from "rxjs";
import { NotificationService } from "../services/notification.service";

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const notificationService = inject(NotificationService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 404) {
        notificationService.showError(
          "Niet gevonden",
          "De gevraagde resource bestaat niet.",
          4000
        );
      } else if (error.status !== 400 && error.status !== 401) {
        notificationService.showError(
          "Er is iets misgegaan",
          "Probeer het later opnieuw of neem contact op met Nanou.",
          5000
        );
      }

      return throwError(() => error);
    })
  );
};
