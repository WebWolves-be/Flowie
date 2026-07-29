import { Component } from "@angular/core";
import { DatePipe, NgClass } from "@angular/common";
import { CdkDropList, CdkDrag, CdkDragHandle } from "@angular/cdk/drag-drop";
import { TaskItemBase } from "./task-item-base";

/**
 * Desktop task card: everything expands in place, and there is room to show it.
 * The mobile design is a separate component — see TaskItemMobileComponent.
 */
@Component({
  selector: "app-task-item",
  standalone: true,
  imports: [NgClass, DatePipe, CdkDropList, CdkDrag, CdkDragHandle],
  templateUrl: "./task-item.component.html",
  styleUrl: "./task-item.component.scss"
})
export class TaskItemComponent extends TaskItemBase {}
