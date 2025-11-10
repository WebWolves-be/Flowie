import { bootstrapApplication } from "@angular/platform-browser";
import { provideRouter, Routes } from "@angular/router";
import { provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { AppComponent } from "./app/app.component";
import { DashboardPage } from "./app/features/dashboard/components/dashboard-page/dashboard-page";
import { TasksPage } from "./app/features/tasks/components/tasks-page/tasks-page";
import { SettingsPage } from "./app/features/settings/components/settings-page/settings-page";

const routes: Routes = [
  {
    path: "",
    pathMatch: "full",
    redirectTo: "dashboard"
  },
  {
    path: "dashboard",
    component: DashboardPage
  },
  {
    path: "taken",
    component: TasksPage
  },
  {
    path: "taken/project/:id",
    component: TasksPage
  },
  {
    path: "instellingen",
    component: SettingsPage
  },
  {
    path: "**",
    redirectTo: "dashboard"
  }
];


bootstrapApplication(AppComponent, {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptorsFromDi())
  ]
}).catch(err => console.error(err));