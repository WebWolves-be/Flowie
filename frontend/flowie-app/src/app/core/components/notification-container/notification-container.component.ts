import { Component, inject } from "@angular/core";
import { CommonModule } from "@angular/common";
import { NotificationService } from "../../services/notification.service";

@Component({
  selector: "app-notification-container",
  standalone: true,
  imports: [CommonModule],
  template: `
    <div class="fixed top-0 left-0 right-0 md:bottom-4 md:top-auto md:left-1/2 md:right-auto md:-translate-x-1/2 z-50 md:max-w-md">
      @for (notification of notifications(); track notification.id) {
        <div
          class="rounded-none md:rounded-lg shadow-lg p-4 flex items-start gap-3 animate-slide-in border border-b-0 md:border-b last:border-b md:mb-2"
          [ngClass]="{
            'bg-red-50 border-red-200': notification.type === 'error',
            'bg-green-50 border-green-200': notification.type === 'success',
            'bg-teal-50 border-teal-200': notification.type === 'info',
            'bg-yellow-50 border-yellow-200': notification.type === 'warning'
          }">

          <div class="flex-shrink-0">
            @if (notification.type === 'error') {
              <i class="fas fa-circle-exclamation w-5 h-5 text-red-600"></i>
            } @else if (notification.type === 'success') {
              <i class="fas fa-circle-check w-5 h-5 text-green-600"></i>
            } @else if (notification.type === 'info') {
              <i class="fas fa-circle-info w-5 h-5 text-teal-600"></i>
            } @else if (notification.type === 'warning') {
              <i class="fas fa-triangle-exclamation w-5 h-5 text-yellow-600"></i>
            }
          </div>

          <div class="flex-1 min-w-0">
            <p
              class="font-semibold text-sm"
              [ngClass]="{
                'text-red-800': notification.type === 'error',
                'text-green-800': notification.type === 'success',
                'text-teal-800': notification.type === 'info',
                'text-yellow-800': notification.type === 'warning'
              }">
              {{ notification.title }}
            </p>
            @if (notification.message) {
              <p
                class="text-sm mt-1"
                [ngClass]="{
                  'text-red-700': notification.type === 'error',
                  'text-green-700': notification.type === 'success',
                  'text-teal-700': notification.type === 'info',
                  'text-yellow-700': notification.type === 'warning'
                }">
                {{ notification.message }}
              </p>
            }
          </div>

          <button
            (click)="remove(notification.id)"
            class="flex-shrink-0 text-gray-400 hover:text-gray-600 transition-colors"
            type="button">
            <i class="fas fa-times w-5 h-5"></i>
          </button>
        </div>
      }
    </div>
  `,
  styles: [`
    @keyframes slide-in {
      from {
        transform: translateY(100%);
        opacity: 0;
      }
      to {
        transform: translateY(0);
        opacity: 1;
      }
    }

    .animate-slide-in {
      animation: slide-in 0.3s ease-out;
    }
  `]
})
export class NotificationContainerComponent {
  #notificationService = inject(NotificationService);

  notifications = this.#notificationService.notifications;

  remove(id: number): void {
    this.#notificationService.remove(id);
  }
}
