import { bootstrapApplication } from "@angular/platform-browser";
import { provideRouter, Routes } from "@angular/router";
import {
  provideHttpClient,
  withInterceptors,
} from "@angular/common/http";
import { AppComponent } from "./app/app.component";
import { errorInterceptor } from "./app/core/interceptors/error.interceptor";

const routes: Routes = [
  {
    path: "",
    pathMatch: "full",
    redirectTo: "dashboard",
  },
  {
    path: "dashboard",
    loadComponent: () =>
      import("./app/features/dashboard/components/dashboard-page/dashboard-page").then(
        (m) => m.DashboardPage
      ),
  },
  {
    path: "taken",
    loadComponent: () =>
      import("./app/features/tasks/components/tasks-page/tasks-page").then(
        (m) => m.TasksPage
      ),
  },
  {
    path: "taken/project/:id",
    loadComponent: () =>
      import("./app/features/tasks/components/tasks-page/tasks-page").then(
        (m) => m.TasksPage
      ),
  },
  {
    path: "instellingen",
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
    provideHttpClient(withInterceptors([errorInterceptor])),
  ],
}).catch((err) => console.error(err));
