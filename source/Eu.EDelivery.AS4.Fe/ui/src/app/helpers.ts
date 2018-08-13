import { AbstractControl, FormGroup } from '@angular/forms';

export function removeError(formGroup: FormGroup, error: string): { [key: string]: any } | null {
    const errors = { ...formGroup.errors };
    if (!!formGroup.errors) {
        delete errors[error];
    }
    if (Object.keys(errors).length === 0) {
        return null;
    }
    return errors;
}

export function manageError(formGroup: AbstractControl, error: string, set: boolean = false) {
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
