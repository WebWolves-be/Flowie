import { HttpInterceptorFn, HttpErrorResponse } from "@angular/common/http";
import { inject } from "@angular/core";
import { catchError, throwError } from "rxjs";
import { AuthFacade } from "../facades/auth.facade";
import { Router } from "@angular/router";

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authFacade = inject(AuthFacade);
  const router = inject(Router);

  const isAuthEndpoint = req.url.includes("/auth/login") || req.url.includes("/auth/refresh");

  if (!isAuthEndpoint) {
    const token = authFacade.getAccessToken();

    if (token) {
      req = req.clone({
        setHeaders: {
          Authorization: `Bearer ${token}`
        }
      });
    }
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !isAuthEndpoint) {
        if (router.url !== "/login") {
          // Clear the in-memory auth state too — only removing the tokens left
          // isAuthenticated() true, so guestGuard bounced /login → /dashboard
          // and stranded the user in a zombie session.
          authFacade.sessionExpired();
          router.navigate(["/login"]);
        }
      }

      return throwError(() => error);
    })
  );
};
