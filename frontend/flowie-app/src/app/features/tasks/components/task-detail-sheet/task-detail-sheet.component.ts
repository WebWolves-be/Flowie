import { Component, HostListener, output } from "@angular/core";
import { DatePipe } from "@angular/common";
import { TaskItemBase } from "../task-item/task-item-base";
import { SheetDragDirective } from "../../../../core/directives/sheet-drag.directive";

/**
 * Task detail for phones, as a bottom sheet.
 *
 * Inline expansion pushed the rest of the list around and could only ever show
 * one task at a time in a column ~265px wide. A sheet gets the full width, keeps
 * the list still underneath, and is dismissed the way users expect on a phone.
 *
 * It extends TaskItemBase so the status rules and subtask handling are the same
 * code the rows and the desktop card use.
 */
@Component({
  selector: "app-task-detail-sheet",
  standalone: true,
  imports: [DatePipe, SheetDragDirective],
  templateUrl: "./task-detail-sheet.component.html",
  styleUrl: "./task-detail-sheet.component.scss"
})
export class TaskDetailSheetComponent extends TaskItemBase {
  close = output<void>();

  @HostListener("document:keydown.escape")
  onEscape(): void {
    this.close.emit();
  }
}
