import { Injectable, signal } from "@angular/core";

export type NotificationType = "error" | "success" | "info" | "warning";

export interface Notification {
  id: number;
  type: NotificationType;
  title: string;
  message?: string;
  duration?: number;
}

@Injectable({
  providedIn: "root"
})
export class NotificationService {
  #notifications = signal<Notification[]>([]);
  #nextId = 0;

  notifications = this.#notifications.asReadonly();

  showSuccess(title: string, message?: string, duration = 3000): void {
    this.#show("success", title, message, duration);
  }

  showInfo(title: string, message?: string, duration = 3000): void {
    this.#show("info", title, message, duration);
  }

  showWarning(title: string, message?: string, duration = 4000): void {
    this.#show("warning", title, message, duration);
  }

  showError(title: string, message?: string, duration = 5000): void {
    this.#show("error", title, message, duration);
  }

  remove(id: number): void {
    this.#notifications.update(notifications =>
      notifications.filter(n => n.id !== id)
    );
  }

  clear(): void {
    this.#notifications.set([]);
  }

  #show(type: NotificationType, title: string, message?: string, duration?: number): void {
    const notification: Notification = {
      id: this.#nextId++,
      type,
      title,
      message,
      duration
    };

    this.#notifications.update(notifications => [...notifications, notification]);

    if (duration) {
      setTimeout(() => this.remove(notification.id), duration);
    }
  }

}
