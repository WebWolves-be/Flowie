import { DestroyRef, Injectable, inject } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { SwUpdate, VersionReadyEvent } from "@angular/service-worker";
import { filter } from "rxjs/operators";
import { NotificationService } from "./notification.service";
import { AppReloader } from "./app-reloader";

const UPDATE_CHECK_INTERVAL_MS = 30 * 60_000;

@Injectable({
  providedIn: "root"
})
export class PwaUpdateService {
  #updates = inject(SwUpdate);
  #notifications = inject(NotificationService);
  #reloader = inject(AppReloader);
  #destroy = inject(DestroyRef);

  #promptShown = false;

  initialize(): void {
    if (!this.#updates.isEnabled) {
      return;
    }

    this.#updates.versionUpdates
      .pipe(
        filter((event): event is VersionReadyEvent => event.type === "VERSION_READY"),
        takeUntilDestroyed(this.#destroy)
      )
      .subscribe(() => this.#promptForUpdate());

    this.#updates.unrecoverable
      .pipe(takeUntilDestroyed(this.#destroy))
      .subscribe(() => this.#promptForRecovery());

    const poll = setInterval(
      () => this.#updates.checkForUpdate().catch(() => undefined),
      UPDATE_CHECK_INTERVAL_MS
    );
    this.#destroy.onDestroy(() => clearInterval(poll));
  }

  async checkNow(): Promise<void> {
    if (!this.#updates.isEnabled) {
      this.#notifications.showInfo(
        "Updates zijn hier niet beschikbaar",
        "De offline versie draait alleen in de geïnstalleerde app."
      );
      return;
    }

    try {
      const found = await this.#updates.checkForUpdate();
      if (!found) {
        this.#notifications.showInfo("Je gebruikt de laatste versie");
      }
    } catch {
      this.#notifications.showError("Kon niet controleren op updates");
    }
  }

  #promptForUpdate(): void {
    if (this.#promptShown) {
      return;
    }
    this.#promptShown = true;

    this.#notifications.showAction(
      "info",
      "Nieuwe versie beschikbaar",
      "Herlaad de app om de laatste versie te gebruiken.",
      {
        label: "Herladen",
        run: () =>
          this.#updates
            .activateUpdate()
            .then(() => this.#reloader.reload())
            .catch(() => this.#reloader.reload())
      }
    );
  }

  #promptForRecovery(): void {
    this.#notifications.showAction(
      "error",
      "De app moet opnieuw geladen worden",
      "Er ging iets mis met de offline versie van Flowie.",
      {
        label: "Herladen",
        run: () => this.#reloader.reload()
      }
    );
  }
}
