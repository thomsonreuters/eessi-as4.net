import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Step } from './Step';
import { SettingForm } from './SettingForm';

export class StepForm {
    public static getForm(formBuilder: FormBuilder, current: Step): FormGroup {
        return formBuilder.group({
            type: [current && current.type],
            unDecorated: [!!(current && current.unDecorated)],
            setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => SettingForm.getForm(formBuilder, item))),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Step) {
        form.removeControl('type');
        form.addControl('type', formBuilder.control(current && current.type));
        form.removeControl('unDecorated');
        form.addControl('unDecorated', formBuilder.control(current && current.unDecorated));

        form.removeControl('setting');
        form.addControl('setting', formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => SettingForm.getForm(formBuilder, item))));
    }
}
