import { inject, Injectable, signal } from "@angular/core";
import { HttpClient } from "@angular/common/http";
import { environment } from "../../../../environments/environment";
import { finalize, map, Observable, tap } from "rxjs";
import { GetCalendarFeedUrlResponse } from "../models/get-calendar-feed-url-response.model";
import { RegenerateCalendarFeedTokenResponse } from "../models/regenerate-calendar-feed-token-response.model";

@Injectable({ providedIn: "root" })
export class CalendarFacade {
  readonly #http = inject(HttpClient);
  readonly #apiUrl = environment.apiUrl;

  #feedUrl = signal<string | null>(null);
  #isLoadingFeedUrl = signal<boolean>(false);

  feedUrl = this.#feedUrl.asReadonly();
  isLoadingFeedUrl = this.#isLoadingFeedUrl.asReadonly();

  getCalendarFeedUrl(): void {
    this.#isLoadingFeedUrl.set(true);

    this.#http
      .get<GetCalendarFeedUrlResponse>(`${this.#apiUrl}/api/calendar/url`)
      .pipe(finalize(() => this.#isLoadingFeedUrl.set(false)))
      .subscribe(response => {
        this.#feedUrl.set(response.feedUrl);
      });
  }

  regenerateToken(): Observable<string> {
    return this.#http
      .post<RegenerateCalendarFeedTokenResponse>(`${this.#apiUrl}/api/calendar/regenerate`, {})
      .pipe(
        tap(response => this.#feedUrl.set(response.feedUrl)),
        map(response => response.feedUrl)
      );
  }
}
