import { FormControl } from '@angular/forms';

let regex = new RegExp(/^[0-9a-fA-F]{40}$/);
export function thumbPrintValidation(control: FormControl, required: boolean = false) {
    if (!required && !!!control.value) return null;
    let value = control.value;
    if (regex.test(value)) return null;
    return {
        validThumbPrint: {
            valid: false
        }
    };
}
