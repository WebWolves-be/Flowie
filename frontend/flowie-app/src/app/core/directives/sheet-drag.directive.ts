import { Directive, ElementRef, inject, input, output } from "@angular/core";

const DISMISS_FRACTION = 0.25;
const FLICK_VELOCITY = 0.5;
const SETTLE_MS = 220;

/**
 * A slow drag has to travel far enough to read as deliberate; a fast one does
 * not, because nobody flicks a sheet downwards by accident. Upward movement can
 * never dismiss — the sheet is already against the top of its travel.
 */
export function shouldDismiss(
  distance: number,
  velocity: number,
  sheetHeight: number
): boolean {
  if (distance <= 0) return false;
  return distance > sheetHeight * DISMISS_FRACTION || velocity > FLICK_VELOCITY;
}

/**
 * Drag-to-dismiss for a bottom sheet.
 *
 * Applied to the grab bar and header only. Putting it on the whole panel would
 * take the gesture away from the scrolling body, so a downward swipe over the
 * task description would close the sheet instead of scrolling it.
 */
@Directive({
  selector: "[appSheetDrag]",
  standalone: true,
  host: {
    "data-sheet-handle": "",
    "(pointerdown)": "onPointerDown($event)",
    "(pointermove)": "onPointerMove($event)",
    "(pointerup)": "onPointerUp($event)",
    "(pointercancel)": "onPointerUp($event)",
    "[style.touch-action]": "'none'"
  }
})
export class SheetDragDirective {
  #host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** The element that actually moves. The handle is only where the drag starts. */
  panel = input.required<HTMLElement>({ alias: "appSheetDrag" });

  dismissed = output<void>();

  #startY = 0;
  #startedAt = 0;
  #dragging = false;

  onPointerDown(event: PointerEvent): void {
    this.#dragging = true;
    this.#startY = event.clientY;
    this.#startedAt = event.timeStamp;
    this.#host.nativeElement.setPointerCapture(event.pointerId);
    this.#setTransition("none");
  }

  onPointerMove(event: PointerEvent): void {
    if (!this.#dragging) return;

    const distance = event.clientY - this.#startY;
    this.#translate(distance > 0 ? distance : distance / 4);
  }

  onPointerUp(event: PointerEvent): void {
    if (!this.#dragging) return;
    this.#dragging = false;

    const distance = event.clientY - this.#startY;
    const elapsed = Math.max(event.timeStamp - this.#startedAt, 1);
    const panel = this.panel();
    const settle = this.#settleMs();

    this.#setTransition(settle ? `transform ${settle}ms cubic-bezier(0.32, 0.72, 0, 1)` : "none");

    if (!shouldDismiss(distance, distance / elapsed, panel.offsetHeight)) {
      this.#translate(0);
      return;
    }

    // The sheet is removed by an `@if` in the parent, so emitting immediately
    // would delete the node mid-animation and the panel would vanish rather
    // than slide out. Let the transform finish first.
    this.#translate(panel.offsetHeight);
    if (!settle) {
      this.dismissed.emit();
      return;
    }
    setTimeout(() => this.dismissed.emit(), settle);
  }

  /** Zero when the user has asked for less motion, which skips both animations. */
  #settleMs(): number {
    return matchMedia("(prefers-reduced-motion: reduce)").matches ? 0 : SETTLE_MS;
  }

  #translate(y: number): void {
    this.panel().style.transform = `translateY(${y}px)`;
  }

  #setTransition(value: string): void {
    this.panel().style.transition = value;
  }
}
