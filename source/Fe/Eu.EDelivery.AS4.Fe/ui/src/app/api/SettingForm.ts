import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Setting } from './Setting';

export class SettingForm {
    public static getForm(formBuilder: FormBuilder, current: Setting): FormGroup {
        return formBuilder.group({
            key: [current && current.key],
            value: [current && current.value],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Setting) {
        form.removeControl('key');
        form.addControl('key', formBuilder.control(current && current.key));
        form.removeControl('value');
        form.addControl('value', formBuilder.control(current && current.value));
    }
}
