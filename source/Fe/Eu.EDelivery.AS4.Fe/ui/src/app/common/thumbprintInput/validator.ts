import { Directive, Input } from '@angular/core';
import { FormControl, Validator, ValidatorFn, AbstractControl } from '@angular/forms';

let regex = new RegExp(/^[0-9a-fA-F]{40}$/);
export function thumbPrintValidation(control: FormControl, required: boolean = false) {
    if (!required && !!!control.value) {
        return null;
    }
    let value = control.value;
    if (regex.test(value)) {
        return null;
    }
    return {
        validThumbPrint: {
            valid: false
        }
    };
}

@Directive({ selector: '[thumbprintvalidator][ngmodel],[thumbprintvalidator][ngFormControl]' })
export class ThumbprintValidatorDirective implements Validator {

    public validate(control: FormControl): any {
        return thumbPrintValidation(control);
    }
}
