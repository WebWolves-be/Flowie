import { Component, signal } from "@angular/core";
import { NgClass } from "@angular/common";
import { TaskTypesSettingsComponent } from "../task-types/task-types-settings.component";
import { CalendarSettingsComponent } from "../calendar-settings/calendar-settings.component";

@Component({
  selector: "app-settings-page",
  standalone: true,
  imports: [NgClass, TaskTypesSettingsComponent, CalendarSettingsComponent],
  templateUrl: "./settings-page.html",
  styleUrl: "./settings-page.scss"
})
export class SettingsPage {
  activeTab = signal<"task-types" | "calendar">("task-types");
}