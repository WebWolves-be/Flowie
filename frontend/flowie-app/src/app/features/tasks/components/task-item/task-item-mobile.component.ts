import { Component, computed, output } from "@angular/core";
import { DatePipe, NgClass } from "@angular/common";
import { TaskItemBase } from "./task-item-base";

/**
 * Mobile task row.
 *
 * Deliberately not a shrunken version of the desktop card: it shows the fields
 * you triage by — due date, assignee, subtask progress — at rest, keeps the
 * status control one tap away, and defers everything else to a detail sheet
 * rather than expanding in place and pushing the rest of the list around.
 */
@Component({
  selector: "app-task-item-mobile",
  standalone: true,
  imports: [NgClass, DatePipe],
  templateUrl: "./task-item-mobile.component.html",
  styleUrl: "./task-item.component.scss"
})
export class TaskItemMobileComponent extends TaskItemBase {
  detailRequested = output<number>();

  completedSubtasks = computed(
    () => (this.task().subtasks ?? []).filter(s => this.isSubtaskDone(s)).length
  );

  onStatusTap(event: Event) {
    event.stopPropagation();
    this.advanceStatus();
  }
}
