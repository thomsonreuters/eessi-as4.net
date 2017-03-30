import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SettingsDatabase } from './SettingsDatabase';

export class SettingsDatabaseForm {
    public static getForm(formBuilder: FormBuilder, current: SettingsDatabase): FormGroup {
        return formBuilder.group({
            provider: [current && current.provider],
            connectionString: [current && current.connectionString],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SettingsDatabase) {
        form.removeControl('provider');
        form.addControl('provider', formBuilder.control(current && current.provider));
        form.removeControl('connectionString');
        form.addControl('connectionString', formBuilder.control(current && current.connectionString));
    }
}
