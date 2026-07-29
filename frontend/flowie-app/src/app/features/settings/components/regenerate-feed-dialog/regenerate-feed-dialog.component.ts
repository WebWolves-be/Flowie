import { Component, inject, signal } from "@angular/core";
import { DialogRef } from "@angular/cdk/dialog";
import { HttpErrorResponse } from "@angular/common/http";
import { catchError, EMPTY } from "rxjs";
import { CalendarFacade } from "../../facade/calendar.facade";
import { NotificationService } from "../../../../core/services/notification.service";
import { extractErrorMessage } from "../../../../core/utils/error-message.util";

@Component({
  selector: "app-regenerate-feed-dialog",
  standalone: true,
  imports: [],
  templateUrl: "./regenerate-feed-dialog.component.html"
})
export class RegenerateFeedDialogComponent {
  #ref = inject(DialogRef);
  #facade = inject(CalendarFacade);
  #notificationService = inject(NotificationService);

  readonly errorMessage = signal<string | null>(null);
  readonly isRegenerating = signal<boolean>(false);

  onCancel(): void {
    this.#ref.close();
  }

  onConfirm(): void {
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
        this.#notificationService.showSuccess("Agenda feed URL succesvol vernieuwd");
        this.#ref.close();
      });
  }
}
