import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Setting } from './Setting';

export class SettingForm {
    public static getForm(formBuilder: FormBuilder, current: Setting): FormGroup {
        return formBuilder.group({
            key: [current && current.key],
            value: [current && current.value],
        });
    }
}
