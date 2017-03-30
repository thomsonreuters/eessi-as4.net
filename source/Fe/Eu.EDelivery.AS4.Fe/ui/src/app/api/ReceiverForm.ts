import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Receiver } from './Receiver';
import { SettingForm } from './SettingForm';

export class ReceiverForm {
    public static getForm(formBuilder: FormBuilder, current: Receiver): FormGroup {
        return formBuilder.group({
            type: [current && current.type],
            setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => SettingForm.getForm(formBuilder, item))),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Receiver) {
        form.removeControl('type');
        form.addControl('type', formBuilder.control(current && current.type));

        form.removeControl('setting');
        form.addControl('setting', formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => SettingForm.getForm(formBuilder, item))));
    }
}