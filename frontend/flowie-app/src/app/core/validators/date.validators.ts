import { AbstractControl, ValidationErrors, ValidatorFn } from "@angular/forms";

export class DateValidators {
  static futureDate(): ValidatorFn {
    return (control: AbstractControl): ValidationErrors | null => {
      if (!control.value) {
        return null;
      }

      const inputDate = new Date(control.value);
      const today = new Date();
      today.setHours(0, 0, 0, 0);

      if (inputDate <= today) {
        return { futureDate: true };
      }

      return null;
    };
  }
}
