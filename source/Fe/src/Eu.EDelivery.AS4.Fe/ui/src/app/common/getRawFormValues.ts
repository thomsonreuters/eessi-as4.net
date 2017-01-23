import { FormGroup, AbstractControl } from '@angular/forms';

export function getRawFormValues(form: FormGroup): any {
    let initValue = {};
    return _getRawFormValues(form, initValue);
}

function _getRawFormValues(form: FormGroup | AbstractControl, value) {
    if (form instanceof FormGroup) {
        Object.keys(form.controls).forEach((control) => {
            value[control] = {};
            value[control] = _getRawFormValues(form.controls[control], value[control]);
            return value;
        });
    } else if (form instanceof AbstractControl) {
        return form.value;
    }

    return value;
}
