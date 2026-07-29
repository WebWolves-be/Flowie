import { Injectable, signal, computed, inject } from "@angular/core";
import { BreakpointObserver, Breakpoints } from "@angular/cdk/layout";
import { toSignal } from "@angular/core/rxjs-interop";

@Injectable({
  providedIn: "root"
})
export class BreakpointService {
  #breakpointObserver = inject(BreakpointObserver);

  #breakpoints = toSignal(
    this.#breakpointObserver.observe([
      "(max-width: 767px)",
      "(min-width: 768px) and (max-width: 1023px)",
      "(min-width: 1024px)"
    ]),
    { initialValue: { matches: false, breakpoints: {} } }
  );

  isMobile = computed(() => {
    const state = this.#breakpoints();
    return (state.breakpoints as Record<string, boolean>)["(max-width: 767px)"] ?? false;
  });

  isTablet = computed(() => {
    const state = this.#breakpoints();
    return (state.breakpoints as Record<string, boolean>)["(min-width: 768px) and (max-width: 1023px)"] ?? false;
  });

  isDesktop = computed(() => {
    const state = this.#breakpoints();
    return (state.breakpoints as Record<string, boolean>)["(min-width: 1024px)"] ?? false;
  });

  /**
   * True below the `lg` breakpoint, which is where the app shows the mobile
   * header and bottom nav. A phone in landscape is wider than 768px but still
   * far too short for the two-pane desktop layout, so single-pane navigation
   * has to key off this rather than `isMobile`.
   */
  isCompact = computed(() => !this.isDesktop());
}
