import { Component, inject, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DashboardFacade } from '../facade/dashboard.facade';

@Component({
  selector: 'app-dashboard-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './dashboard-page.html',
  styleUrl: './dashboard-page.scss',
})
export class DashboardPage {
  facade = inject(DashboardFacade);
  metrics = this.facade.metrics;
  isLoading = this.facade.isLoading;

  constructor() {
    this.facade.load();
  }
}
