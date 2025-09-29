import {bootstrapApplication} from '@angular/platform-browser';
import {provideAnimations} from '@angular/platform-browser/animations';
import {provideRouter, Routes} from '@angular/router';
import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {APP_INITIALIZER} from '@angular/core';
import {AppComponent} from './app/app.component';
import {LoginComponent} from "./app/features/auth/pages/login/login.component";
import {HomeComponent} from "./app/features/dashboard/pages/home/home.component";
import {AuthGuard} from "./app/core/auth.guard";
import {AuthService} from "./app/features/auth/services/auth.service";
import {firstValueFrom} from 'rxjs';

const routes: Routes = [
    {path: 'login', component: LoginComponent},
    {
        path: 'dashboard',
        component: HomeComponent,
        canActivate: [AuthGuard]
    },
    {path: '', pathMatch: 'full', redirectTo: 'dashboard'},
    {path: '**', redirectTo: 'dashboard'}
];

function initializeApp(authService: AuthService) {
    return () => {
        console.log('🚀 APP_INITIALIZER: Starting session check...');
        console.log('🚀 APP_INITIALIZER: Current browser cookies:', document.cookie);
        
        return firstValueFrom(authService.checkSession()).then(result => {
            console.log('🚀 APP_INITIALIZER: Session check completed with result:', result);
            if (result) {
                console.log('✅ APP_INITIALIZER: User is authenticated');
            } else {
                console.log('ℹ️ APP_INITIALIZER: User is not authenticated (this is normal for first visit)');
            }
            return result;
        }).catch(error => {
            console.error('🚀 APP_INITIALIZER: Session check failed with error:', error);
            // Don't fail app initialization just because session check failed
            console.log('ℹ️ APP_INITIALIZER: Continuing with unauthenticated state');
            return false;
        });
    };
}

bootstrapApplication(AppComponent, {
    providers: [
        provideAnimations(),
        provideRouter(routes),
        provideHttpClient(withInterceptorsFromDi()),
        {
            provide: APP_INITIALIZER,
            useFactory: initializeApp,
            deps: [AuthService],
            multi: true
        }
    ]
}).catch(err => console.error(err));