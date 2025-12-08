import { Component, computed, inject } from '@angular/core';
import { RouterOutlet, RouterLink, RouterLinkActive } from '@angular/router';
import { NotificationContainerComponent } from './core/components/notification-container/notification-container.component';
import { AuthFacade } from './core/facades/auth.facade';
import { NotificationService } from './core/services/notification.service';
import { catchError, EMPTY } from 'rxjs';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, NotificationContainerComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  #authFacade = inject(AuthFacade);
  #notificationService = inject(NotificationService);

  isAuthenticated = this.#authFacade.isAuthenticated;
  currentUser = this.#authFacade.currentUser;

  userInitials = computed(() => {
    const user = this.currentUser();
    if (!user?.name) return '';

    const parts = user.name.trim().split(' ');
    if (parts.length === 1) {
      return parts[0].substring(0, 2).toUpperCase();
    }

    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  });

  onLogout(): void {
    this.#authFacade.logout()
      .pipe(
        catchError(() => {
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.#notificationService.showSuccess('U bent uitgelogd');
      });
  }
}
