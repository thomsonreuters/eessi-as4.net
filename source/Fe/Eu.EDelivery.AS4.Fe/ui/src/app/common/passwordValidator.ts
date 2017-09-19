import { FormControl } from '@angular/forms';

// tslint:disable-next-line:max-line-length
let regex: RegExp = new RegExp(/^(?=(?:.*[A-Z]){1,})(?=(?:.*[a-z]){1,})(?=(?:.*\d){1,})(?=(?:.*[!@#$%^&*()\/\\\-_=+{};:,<.>é"'§èçà°|[\]µ~?`´]){1,})([A-Za-z0-9!@#$%^&*()\/\\\-_=+{};:,<.>é"'§èçà°|[\]µ~?`´]{8,})$/);
export function validatePassword(control: FormControl): any {
    if (!!!control.value) {
        return null;
    } else {
        if ((<string>control.value).length === 0 || !regex.test(control.value)) {
            return {
                validatePassword: {
                    valid: false
                }
            };
        }
    }
}
