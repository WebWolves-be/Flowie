import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError, BehaviorSubject, filter, take } from 'rxjs';
import { TokenService } from '../services/token.service';

/**
 * JWT HTTP Interceptor that automatically:
 * - Attaches JWT tokens to outgoing requests
 * - Handles token refresh on 401 errors
 * - Prevents multiple simultaneous refresh attempts
 * - Implements proper error handling
 */
export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenService = inject(TokenService);
  
  // Skip token attachment for auth endpoints to avoid circular dependencies
  if (isAuthEndpoint(req.url)) {
    return next(req);
  }
  
  const accessToken = tokenService.getAccessToken();
  
  // Clone request and add authorization header if token exists
  const authReq = accessToken
    ? req.clone({
        setHeaders: {
          Authorization: `Bearer ${accessToken}`
        }
      })
    : req;
  
  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      // Handle 401 Unauthorized errors
      if (error.status === 401 && !isAuthEndpoint(req.url)) {
        return handle401Error(req, next, tokenService);
      }
      
      return throwError(() => error);
    })
  );
};

// Subject to track ongoing refresh attempts
let isRefreshing = false;
const refreshTokenSubject: BehaviorSubject<string | null> = new BehaviorSubject<string | null>(null);

/**
 * Handle 401 errors by attempting token refresh
 */
function handle401Error(req: any, next: any, tokenService: TokenService) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);
    
    console.log('🔄 JwtInterceptor: Attempting token refresh due to 401 error');
    
    return tokenService.refreshTokens().pipe(
      switchMap((tokenResponse) => {
        isRefreshing = false;
        
        if (tokenResponse && tokenResponse.accessToken) {
          refreshTokenSubject.next(tokenResponse.accessToken);
          
          // Retry the original request with new token
          const retryReq = req.clone({
            setHeaders: {
              Authorization: `Bearer ${tokenResponse.accessToken}`
            }
          });
          
          console.log('✅ JwtInterceptor: Token refreshed, retrying request');
          return next(retryReq);
        } else {
          // Refresh failed, clear tokens and redirect to login
          console.log('❌ JwtInterceptor: Token refresh failed, clearing tokens');
          tokenService.clearTokens();
          // You can emit an event here to redirect to login page
          // or handle this in your auth service
          return throwError(() => new Error('Token refresh failed'));
        }
      }),
      catchError((error) => {
        isRefreshing = false;
        tokenService.clearTokens();
        console.error('❌ JwtInterceptor: Token refresh error:', error);
        return throwError(() => error);
      })
    );
  } else {
    // If a refresh is already in progress, wait for it to complete
    return refreshTokenSubject.pipe(
      filter(token => token !== null),
      take(1),
      switchMap(token => {
        const retryReq = req.clone({
          setHeaders: {
            Authorization: `Bearer ${token}`
          }
        });
        
        return next(retryReq);
      })
    );
  }
}

/**
 * Check if the URL is an authentication endpoint
 */
function isAuthEndpoint(url: string): boolean {
  const authEndpoints = [
    '/auth/login',
    '/auth/refresh',
    '/auth/logout',
    '/auth/register'
  ];
  
  return authEndpoints.some(endpoint => url.includes(endpoint));
}

/**
 * Security headers interceptor to add additional security headers
 */
export const securityHeadersInterceptor: HttpInterceptorFn = (req, next) => {
  const secureReq = req.clone({
    setHeaders: {
      'X-Requested-With': 'XMLHttpRequest',
      'Cache-Control': 'no-cache, no-store, must-revalidate',
      'Pragma': 'no-cache',
      'Expires': '0'
    }
  });
  
  return next(secureReq);
};