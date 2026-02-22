import { Component, inject, OnInit, signal } from "@angular/core";
import { CalendarFacade } from "../../facade/calendar.facade";
import { catchError, EMPTY } from "rxjs";
import { HttpErrorResponse } from "@angular/common/http";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";
import { NotificationService } from "../../../../core/services/notification.service";
import { Dialog } from "@angular/cdk/dialog";

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
  isRegenerating = signal<boolean>(false);

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

  regenerateToken(): void {
    const confirmed = confirm(
      "Weet je zeker dat je de agenda feed URL wilt vernieuwen? De oude URL zal niet meer werken."
    );

    if (!confirmed) {
      return;
    }

    this.errorMessage.set(null);
    this.isRegenerating.set(true);

    this.#facade
      .regenerateToken()
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(extractErrorMessage(error));
          this.isRegenerating.set(false);
          return EMPTY;
        })
      )
      .subscribe(() => {
        this.isRegenerating.set(false);
        this.#notifications.showSuccess("Agenda feed URL succesvol vernieuwd");
      });
  }
}
