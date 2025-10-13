import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { map, catchError, of } from 'rxjs';
import { AuthService } from '../services/auth.service';
import { TokenService } from '../services/token.service';

/**
 * Authentication guard that:
 * - Checks for valid JWT tokens
 * - Validates tokens with the server
 * - Redirects to login if authentication fails
 * - Handles token refresh automatically
 */
export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const router = inject(Router);
  
  console.log('🔒 AuthGuard: Checking authentication for route:', state.url);
  
  // First check if we have valid tokens
  if (!tokenService.hasValidTokens()) {
    console.log('❌ AuthGuard: No valid tokens, redirecting to login');
    router.navigate(['/auth/login'], { 
      queryParams: { returnUrl: state.url },
      replaceUrl: true 
    });
    return false;
  }
  
  // Validate session with server
  return authService.checkSession().pipe(
    map(isAuthenticated => {
      if (isAuthenticated) {
        console.log('✅ AuthGuard: Authentication successful');
        return true;
      } else {
        console.log('❌ AuthGuard: Authentication failed, redirecting to login');
        router.navigate(['/auth/login'], { 
          queryParams: { returnUrl: state.url },
          replaceUrl: true 
        });
        return false;
      }
    }),
    catchError(error => {
      console.error('❌ AuthGuard: Authentication check error:', error);
      router.navigate(['/auth/login'], { 
        queryParams: { returnUrl: state.url },
        replaceUrl: true 
      });
      return of(false);
    })
  );
};

/**
 * Guest guard that prevents authenticated users from accessing auth pages
 */
export const guestGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const tokenService = inject(TokenService);
  const router = inject(Router);
  
  console.log('🚫 GuestGuard: Checking guest access for route:', state.url);
  
  // If user has valid tokens and is authenticated, redirect to dashboard
  if (tokenService.hasValidTokens() && authService.isCurrentlyAuthenticated()) {
    console.log('ℹ️ GuestGuard: User is authenticated, redirecting to dashboard');
    router.navigate(['/dashboard'], { replaceUrl: true });
    return false;
  }
  
  console.log('✅ GuestGuard: Guest access allowed');
  return true;
};

/**
 * Role-based guard (example for future extension)
 */
export const roleGuard = (requiredRoles: string[]): CanActivateFn => {
  return (route, state) => {
    const authService = inject(AuthService);
    const tokenService = inject(TokenService);
    const router = inject(Router);
    
    console.log('💼 RoleGuard: Checking roles:', requiredRoles, 'for route:', state.url);
    
    if (!tokenService.hasValidTokens()) {
      console.log('❌ RoleGuard: No valid tokens, redirecting to login');
      router.navigate(['/auth/login'], { 
        queryParams: { returnUrl: state.url } 
      });
      return false;
    }
    
    const accessToken = tokenService.getAccessToken();
    if (!accessToken) {
      console.log('❌ RoleGuard: No access token, redirecting to login');
      router.navigate(['/auth/login'], { 
        queryParams: { returnUrl: state.url } 
      });
      return false;
    }
    
    const payload = tokenService.parseTokenPayload(accessToken);
    if (!payload) {
      console.log('❌ RoleGuard: Invalid token payload, redirecting to login');
      router.navigate(['/auth/login'], { 
        queryParams: { returnUrl: state.url } 
      });
      return false;
    }
    
    // Here you would check user roles from the token or make an API call
    // For now, we'll assume all authenticated users have access
    console.log('✅ RoleGuard: Access granted');
    return true;
  };
};