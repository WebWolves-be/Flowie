import { Component, inject, OnInit } from "@angular/core";
import { Dialog } from "@angular/cdk/dialog";
import { TaskType } from "../../models/task-type.model";
import { TaskTypeFacade } from "../../facade/task-type.facade";
import { CreateTaskTypeDialogComponent } from "../create-task-type-dialog/create-task-type-dialog.component";
import { DeleteTaskTypeDialogComponent } from "../delete-task-type-dialog/delete-task-type-dialog.component";
import { DeleteTaskTypeDialogData } from "../../models/delete-task-type-dialog-data.model";

@Component({
  selector: "app-task-types-settings",
  standalone: true,
  imports: [],
  templateUrl: "./task-types-settings.component.html",
  styleUrl: "./task-types-settings.component.scss"
})
export class TaskTypesSettingsComponent implements OnInit {
  #facade = inject(TaskTypeFacade);
  #dialog = inject(Dialog);

  taskTypes = this.#facade.taskTypes;
  isTaskTypesLoading = this.#facade.isLoadingTaskTypes;

  ngOnInit(): void {
    this.#facade.getTaskTypes();
  }

  openCreateDialog(): void {
    this.#dialog.open(CreateTaskTypeDialogComponent, {
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }

  openDeleteDialog(taskType: TaskType): void {
    this.#dialog.open(DeleteTaskTypeDialogComponent, {
      data: { taskType } as DeleteTaskTypeDialogData,
      backdropClass: ["fixed", "inset-0", "bg-black/40"],
      panelClass: ["dialog-panel", "flex", "items-center", "justify-center"]
    });
  }
}
