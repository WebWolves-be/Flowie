import { TestBed } from "@angular/core/testing";
import { SwUpdate, VersionEvent, UnrecoverableStateEvent } from "@angular/service-worker";
import { Subject } from "rxjs";
import { PwaUpdateService } from "./pwa-update.service";
import { AppReloader } from "./app-reloader";
import { NotificationService } from "./notification.service";

class SwUpdateStub {
  isEnabled = true;
  versionUpdates = new Subject<VersionEvent>();
  unrecoverable = new Subject<UnrecoverableStateEvent>();
  activateUpdateCalls = 0;
  checkCalls = 0;

  activateUpdate(): Promise<boolean> {
    this.activateUpdateCalls++;
    return Promise.resolve(true);
  }

  checkForUpdate(): Promise<boolean> {
    this.checkCalls++;
    return Promise.resolve(false);
  }
}

class ReloaderStub {
  calls = 0;
  reload(): void {
    this.calls++;
  }
}

function versionReady(): VersionEvent {
  return {
    type: "VERSION_READY",
    currentVersion: { hash: "old" },
    latestVersion: { hash: "new" }
  } as VersionEvent;
}

describe("PwaUpdateService", () => {
  let swUpdate: SwUpdateStub;
  let reloader: ReloaderStub;
  let notifications: NotificationService;
  let service: PwaUpdateService;

  beforeEach(() => {
    swUpdate = new SwUpdateStub();
    reloader = new ReloaderStub();

    TestBed.configureTestingModule({
      providers: [
        { provide: SwUpdate, useValue: swUpdate },
        { provide: AppReloader, useValue: reloader }
      ]
    });

    notifications = TestBed.inject(NotificationService);
    service = TestBed.inject(PwaUpdateService);
  });

  it("prompts with a reload action when a new version is ready", () => {
    service.initialize();

    swUpdate.versionUpdates.next(versionReady());

    expect(notifications.notifications().length).toBe(1);
    expect(notifications.notifications()[0].action?.label).toBe("Herladen");
  });

  it("ignores version events that are not VERSION_READY", () => {
    service.initialize();

    swUpdate.versionUpdates.next({
      type: "VERSION_DETECTED",
      version: { hash: "new" }
    } as VersionEvent);

    expect(notifications.notifications().length).toBe(0);
  });

  it("activates the update and reloads when the action is taken", async () => {
    service.initialize();
    swUpdate.versionUpdates.next(versionReady());

    notifications.notifications()[0].action!.run();
    await Promise.resolve();
    await Promise.resolve();

    expect(swUpdate.activateUpdateCalls).toBe(1);
    expect(reloader.calls).toBe(1);
  });

  it("offers a recovery reload when the service worker state is unrecoverable", () => {
    service.initialize();

    swUpdate.unrecoverable.next({
      type: "UNRECOVERABLE_STATE",
      reason: "cache miss"
    } as UnrecoverableStateEvent);

    const notification = notifications.notifications()[0];
    expect(notification.type).toBe("error");
    expect(notification.action?.label).toBe("Herladen");

    notification.action!.run();
    expect(reloader.calls).toBe(1);
  });

  it("polls for updates on an interval so a long-lived session notices", () => {
    jasmine.clock().install();

    service.initialize();
    expect(swUpdate.checkCalls).toBe(0);

    jasmine.clock().tick(30 * 60_000 + 1);
    expect(swUpdate.checkCalls).toBe(1);

    jasmine.clock().tick(30 * 60_000);
    expect(swUpdate.checkCalls).toBe(2);

    jasmine.clock().uninstall();
  });

  it("does nothing at all when the service worker is disabled", () => {
    jasmine.clock().install();
    swUpdate.isEnabled = false;

    service.initialize();
    swUpdate.versionUpdates.next(versionReady());
    jasmine.clock().tick(60 * 60_000);

    expect(notifications.notifications().length).toBe(0);
    expect(swUpdate.checkCalls).toBe(0);

    jasmine.clock().uninstall();
  });

  it("reports being up to date when a manual check finds nothing", async () => {
    await service.checkNow();

    expect(swUpdate.checkCalls).toBe(1);
    expect(notifications.notifications()[0].title).toBe("Je gebruikt de laatste versie");
  });

  it("stays quiet on a manual check that finds an update, leaving the prompt to announce it", async () => {
    swUpdate.checkForUpdate = () => {
      swUpdate.checkCalls++;
      return Promise.resolve(true);
    };

    await service.checkNow();

    expect(notifications.notifications().length).toBe(0);
  });

  it("tells the user when update checks are unavailable in this environment", async () => {
    swUpdate.isEnabled = false;

    await service.checkNow();

    expect(swUpdate.checkCalls).toBe(0);
    expect(notifications.notifications()[0].title).toBe("Updates zijn hier niet beschikbaar");
  });

  it("prompts only once while the same update is pending", () => {
    service.initialize();

    swUpdate.versionUpdates.next(versionReady());
    swUpdate.versionUpdates.next(versionReady());

    expect(notifications.notifications().length).toBe(1);
  });
});
