import { bootstrapApplication } from "@angular/platform-browser";
import { provideRouter, Routes } from "@angular/router";
import {
  provideHttpClient,
  withInterceptors,
} from "@angular/common/http";
import { registerLocaleData } from "@angular/common";
import localeNl from "@angular/common/locales/nl";
import { AppComponent } from "./app/app.component";
import { authInterceptor } from "./app/core/interceptors/auth.interceptor";
import { errorInterceptor } from "./app/core/interceptors/error.interceptor";
import { authGuard } from "./app/core/guards/auth.guard";
import { guestGuard } from "./app/core/guards/guest.guard";

registerLocaleData(localeNl);

const routes: Routes = [
  {
    path: "",
    pathMatch: "full",
    redirectTo: "dashboard",
  },
  {
    path: "login",
    canActivate: [guestGuard],
    loadComponent: () =>
      import("./app/features/auth/components/login-page/login-page.component").then(
        (m) => m.LoginPageComponent
      ),
  },
  {
    path: "register",
    canActivate: [guestGuard],
    loadComponent: () =>
      import("./app/features/auth/components/register-page/register-page.component").then(
        (m) => m.RegisterPageComponent
      ),
  },
  {
    path: "dashboard",
    canActivate: [authGuard],
    loadComponent: () =>
      import("./app/features/dashboard/components/dashboard-page/dashboard-page").then(
        (m) => m.DashboardPage
      ),
  },
  {
    path: "taken",
    canActivate: [authGuard],
    loadComponent: () =>
      import("./app/features/tasks/components/tasks-page/tasks-page").then(
        (m) => m.TasksPage
      ),
  },
  {
    path: "taken/project/:id",
    canActivate: [authGuard],
    loadComponent: () =>
      import("./app/features/tasks/components/tasks-page/tasks-page").then(
        (m) => m.TasksPage
      ),
  },
  {
    path: "instellingen",
    canActivate: [authGuard],
    loadComponent: () =>
      import("./app/features/settings/components/settings-page/settings-page").then(
        (m) => m.SettingsPage
      ),
  },
  {
    path: "**",
    redirectTo: "dashboard",
  },
];

bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([authInterceptor, errorInterceptor])),
  ],
}).catch((err) => console.error(err));
