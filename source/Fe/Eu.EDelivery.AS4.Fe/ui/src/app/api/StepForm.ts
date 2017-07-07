import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Step } from './Step';
import { SettingForm } from './SettingForm';

export class StepForm {
    public static getForm(formBuilder: FormBuilder, current: Step | null): FormGroup {
        return formBuilder.group({
            type: [current && current.type],
            setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => SettingForm.getForm(formBuilder, item))),
        });
    }
}
