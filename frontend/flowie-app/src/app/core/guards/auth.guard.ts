import { inject } from "@angular/core";
import { CanActivateFn, Router } from "@angular/router";
import { AuthFacade } from "../facades/auth.facade";

export const authGuard: CanActivateFn = () => {
  const authFacade = inject(AuthFacade);
  const router = inject(Router);

  if (authFacade.isAuthenticated()) {
    return true;
  }

  router.navigate(["/login"]);
  return false;
};
