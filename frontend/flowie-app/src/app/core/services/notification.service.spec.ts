import { TestBed } from "@angular/core/testing";
import { NotificationService } from "./notification.service";

describe("NotificationService", () => {
  let service: NotificationService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(NotificationService);
  });

  it("keeps an action notification on screen instead of auto-dismissing it", () => {
    jasmine.clock().install();

    service.showAction("info", "Nieuwe versie", "Herlaad de app", {
      label: "Herladen",
      run: () => {}
    });

    jasmine.clock().tick(60_000);

    expect(service.notifications().length).toBe(1);

    jasmine.clock().uninstall();
  });

  it("exposes the action so the container can render a button for it", () => {
    let ran = false;

    service.showAction("info", "Nieuwe versie", undefined, {
      label: "Herladen",
      run: () => (ran = true)
    });

    const action = service.notifications()[0].action;
    expect(action?.label).toBe("Herladen");

    action?.run();
    expect(ran).toBe(true);
  });

  it("still auto-dismisses ordinary notifications", () => {
    jasmine.clock().install();

    service.showSuccess("Opgeslagen");
    expect(service.notifications().length).toBe(1);

    jasmine.clock().tick(3_001);
    expect(service.notifications().length).toBe(0);

    jasmine.clock().uninstall();
  });
});
