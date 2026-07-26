import { Component, computed, HostListener, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { catchError, EMPTY } from 'rxjs';
import { AuthFacade } from '../../facades/auth.facade';
import { NotificationService } from '../../services/notification.service';

@Component({
  selector: 'app-mobile-header',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './mobile-header.component.html'
})
export class MobileHeaderComponent {
  #authFacade = inject(AuthFacade);
  #notificationService = inject(NotificationService);

  currentUser = this.#authFacade.currentUser;
  showMenu = signal<boolean>(false);

  userInitials = computed(() => {
    const name = this.currentUser()?.name?.trim();
    if (!name) return '';

    const parts = name.split(/\s+/);
    if (parts.length === 1) return parts[0].substring(0, 2).toUpperCase();

    return (parts[0][0] + parts[parts.length - 1][0]).toUpperCase();
  });

  toggleMenu(event: Event): void {
    event.stopPropagation();
    this.showMenu.update(v => !v);
  }

  @HostListener('document:click')
  closeMenu(): void {
    this.showMenu.set(false);
  }

  onLogout(): void {
    this.showMenu.set(false);
    this.#authFacade.logout()
      .pipe(catchError(() => EMPTY))
      .subscribe(() => {
        this.#notificationService.showSuccess('U bent uitgelogd');
      });
  }
}
