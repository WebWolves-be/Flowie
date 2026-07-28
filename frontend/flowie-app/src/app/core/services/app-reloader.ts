import { Injectable } from "@angular/core";

@Injectable({
  providedIn: "root"
})
export class AppReloader {
  reload(): void {
    location.reload();
  }
}
