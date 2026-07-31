import { shouldDismiss } from "./sheet-drag.directive";

describe("shouldDismiss", () => {
  const height = 600;

  it("keeps the sheet open for a short slow drag", () => {
    expect(shouldDismiss(40, 0.05, height)).toBe(false);
  });

  it("dismisses once the sheet is dragged past a quarter of its height", () => {
    expect(shouldDismiss(200, 0.05, height)).toBe(true);
  });

  it("dismisses a short flick, because speed is intent", () => {
    expect(shouldDismiss(40, 0.9, height)).toBe(true);
  });

  it("ignores upward movement entirely", () => {
    expect(shouldDismiss(-300, 2, height)).toBe(false);
  });
});
