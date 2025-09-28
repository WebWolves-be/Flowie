import {bootstrapApplication} from '@angular/platform-browser';
import {provideAnimations} from '@angular/platform-browser/animations';
import {provideRouter, Routes} from '@angular/router';
import {provideHttpClient, withInterceptorsFromDi} from '@angular/common/http';
import {AppComponent} from './app/app.component';
import {LoginComponent} from "./app/features/auth/pages/login/login.component";
import {HomeComponent} from "./app/features/dashboard/pages/home/home-component";
import {AuthGuard} from "./app/core/auth.guard";

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

bootstrapApplication(AppComponent, {
    providers: [
        provideAnimations(),
        provideRouter(routes),
        provideHttpClient(withInterceptorsFromDi()),
    ]
}).catch(err => console.error(err));