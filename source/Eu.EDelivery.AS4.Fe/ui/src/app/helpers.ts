import {
  AbstractControl,
  FormGroup,
  FormControl,
  FormArray
} from '@angular/forms';

export function removeError(
  formGroup: FormGroup,
  error: string
): { [key: string]: any } | null {
  const errors = { ...formGroup.errors };
  if (!!formGroup.errors) {
    delete errors[error];
  }
  if (Object.keys(errors).length === 0) {
    return null;
  }
  return errors;
}

export function manageError(
  formGroup: AbstractControl,
  error: string,
  set: boolean = false
) {
  if (set) {
    formGroup.setErrors({ [error]: true }, { emitEvent: false });
    return;
  }
  if (!formGroup.errors) {
    return;
  }
  if (!!formGroup.errors[error]) {
    formGroup.setErrors(null, { emitEvent: false });
    formGroup.updateValueAndValidity({ onlySelf: true, emitEvent: false });
  }
}

export function triggerFormValidation(
  control: AbstractControl,
  setTouched = true
) {
  if (control instanceof FormControl) {
    markFormAs(setTouched, control);
  } else if (control instanceof FormGroup) {
    markFormAs(setTouched, control);
    Object.keys(control.controls).forEach((field: string) => {
      const groupControl = control.get(field);
      if (groupControl) {
        triggerFormValidation(groupControl);
      }
    });
  } else if (control instanceof FormArray) {
    const controlAsFormArray = control as FormArray;
    controlAsFormArray.controls.forEach((arrayControl: AbstractControl) =>
      triggerFormValidation(arrayControl)
    );
  }
  control.updateValueAndValidity();
}

function markFormAs(touched = true, control: AbstractControl) {
  if (touched) {
    control.markAsTouched({ onlySelf: true });
    control.markAsDirty({ onlySelf: true });
  } else {
    control.markAsPristine({ onlySelf: true });
    control.markAsUntouched({ onlySelf: true });
  }
}
