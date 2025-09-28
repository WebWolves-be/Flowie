import {inject, Injectable} from '@angular/core';
import {CanActivate, Router} from '@angular/router';
import {map, Observable} from 'rxjs';
import {AuthService} from '../features/auth/services/auth.service';

@Injectable({
    providedIn: 'root'
})
export class AuthGuard implements CanActivate {
    #authService = inject(AuthService);
    #router = inject(Router);

    canActivate(): Observable<boolean> | boolean {
        const isAuthenticated = this.#authService.isAuthenticated();

        if (isAuthenticated) {
            return true;
        }

        return this.#authService.checkSession().pipe(
            map(isAuthenticated => {
                if (isAuthenticated) {
                    return true;
                }

                void this.#router.navigate(['/login']);

                return false;
            })
        );
    }
}