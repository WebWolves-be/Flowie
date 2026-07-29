import { Component, inject, OnInit, signal } from "@angular/core";
import { CalendarFacade } from "../../facade/calendar.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { Dialog } from "@angular/cdk/dialog";
import { RegenerateFeedDialogComponent } from "../regenerate-feed-dialog/regenerate-feed-dialog.component";

@Component({
  selector: "app-calendar-settings",
  standalone: true,
  imports: [],
  templateUrl: "./calendar-settings.component.html",
  styleUrl: "./calendar-settings.component.scss"
})
export class CalendarSettingsComponent implements OnInit {
  #facade = inject(CalendarFacade);
  #notifications = inject(NotificationService);
  #dialog = inject(Dialog);

  feedUrl = this.#facade.feedUrl;
  isLoadingFeedUrl = this.#facade.isLoadingFeedUrl;
  errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.#facade.getCalendarFeedUrl();
  }

  copyToClipboard(): void {
    const url = this.feedUrl();
    if (url) {
      navigator.clipboard.writeText(url).then(() => {
        this.#notifications.showSuccess("Agenda feed URL gekopieerd");
      });
    }
  }

  // A native confirm() renders as a system alert titled with the origin, which
  // looks broken inside an installed PWA — so this uses the same CDK dialog as
  // every other confirmation in the app.
  openRegenerateDialog(): void {
    this.#dialog.open(RegenerateFeedDialogComponent, {
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }
}
