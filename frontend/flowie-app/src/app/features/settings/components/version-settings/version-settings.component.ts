import { Component, inject, signal } from "@angular/core";
import { buildInfo } from "../../../../../build-info";
import { PwaUpdateService } from "../../../../core/services/pwa-update.service";

@Component({
  selector: "app-version-settings",
  standalone: true,
  template: `
    <div class="min-w-0">
      <h2 class="text-base font-semibold text-gray-900">Versie</h2>
      <p class="mt-1 text-sm text-gray-600">
        Handig als je wil weten of je de laatste aanpassingen al hebt.
      </p>

      <dl class="mt-4 divide-y divide-gray-200 border-y border-gray-200">
        <div class="flex items-center justify-between gap-4 py-3 min-w-0">
          <dt class="text-sm text-gray-600 flex-shrink-0">Versie</dt>
          <dd id="buildVersion" class="text-sm font-mono font-semibold text-gray-900 min-w-0 break-words text-right">{{ version }}</dd>
        </div>
      </dl>

      <button
        type="button"
        (click)="checkForUpdates()"
        [disabled]="isChecking()"
        class="mt-4 inline-flex items-center justify-center px-4 min-h-touch text-sm rounded-lg bg-teal-600 text-white hover:bg-teal-500 disabled:opacity-50">
        Controleer op updates
      </button>
    </div>
  `
})
export class VersionSettingsComponent {
  #updates = inject(PwaUpdateService);

  version = buildInfo.version;

  isChecking = signal(false);

  async checkForUpdates(): Promise<void> {
    this.isChecking.set(true);
    try {
      await this.#updates.checkNow();
    } finally {
      this.isChecking.set(false);
    }
  }
}
