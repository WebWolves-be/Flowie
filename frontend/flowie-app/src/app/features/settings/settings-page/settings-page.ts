import { Component, signal } from '@angular/core';
import { NgClass, NgIf } from '@angular/common';
import { TaskTypesSettingsComponent } from "../components/task-types/task-types-settings.component";

@Component({
  selector: 'app-settings-page',
  standalone: true,
  imports: [NgClass, NgIf, TaskTypesSettingsComponent],
  templateUrl: './settings-page.html',
  styleUrl: './settings-page.scss',
})
export class SettingsPage {
  activeTab = signal<'task-types'>('task-types');
}