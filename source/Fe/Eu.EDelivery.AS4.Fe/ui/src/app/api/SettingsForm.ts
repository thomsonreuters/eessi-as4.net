import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Settings } from './Settings';
import { SettingsDatabaseForm } from './SettingsDatabaseForm';
import { CertificateStoreForm } from './CertificateStoreForm';
import { CustomSettingsForm } from './CustomSettingsForm';
import { SettingsAgentsForm } from './SettingsAgentsForm';

export class SettingsForm {
    public static getForm(formBuilder: FormBuilder, current: Settings): FormGroup {
        return formBuilder.group({
            idFormat: [current && current.idFormat],
            database: SettingsDatabaseForm.getForm(formBuilder, current && current.database),
            certificateStore: CertificateStoreForm.getForm(formBuilder, current && current.certificateStore),
            customSettings: CustomSettingsForm.getForm(formBuilder, current && current.customSettings),
            agents: SettingsAgentsForm.getForm(formBuilder, current && current.agents),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Settings) {
        form.removeControl('idFormat');
        form.addControl('idFormat', formBuilder.control(current && current.idFormat));

        form.removeControl('database');
        form.addControl('database', SettingsDatabaseForm.getForm(formBuilder, current && current.database));
        form.removeControl('certificateStore');
        form.addControl('certificateStore', CertificateStoreForm.getForm(formBuilder, current && current.certificateStore));
        form.removeControl('customSettings');
        form.addControl('customSettings', CustomSettingsForm.getForm(formBuilder, current && current.customSettings));
        form.removeControl('agents');
        form.addControl('agents', SettingsAgentsForm.getForm(formBuilder, current && current.agents));
    }
}
