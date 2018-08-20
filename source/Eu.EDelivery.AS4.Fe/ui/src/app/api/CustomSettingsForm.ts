import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CustomSettings } from './CustomSettings';
import { SettingForm } from './SettingForm';

export class CustomSettingsForm {
    public static getForm(formBuilder: FormBuilder, current: CustomSettings): FormGroup {
        return formBuilder.group({
            setting: formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => SettingForm.getForm(formBuilder, item))),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: CustomSettings) {
        form.removeControl(CustomSettings.FIELD_setting);
        form.addControl(CustomSettings.FIELD_setting, formBuilder.array(!!!(current && current.setting) ? [] : current.setting.map(item => SettingForm.getForm(formBuilder, item))));
    }
}
