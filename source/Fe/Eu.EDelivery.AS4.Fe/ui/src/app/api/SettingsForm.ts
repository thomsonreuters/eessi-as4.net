import { FormWrapper } from './../common/form.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Settings } from './Settings';
import { SettingsDatabaseForm } from './SettingsDatabaseForm';
import { CertificateStoreForm } from './CertificateStoreForm';
import { CustomSettingsForm } from './CustomSettingsForm';
import { SettingsAgentsForm } from './SettingsAgentsForm';

export class SettingsForm {
    public static getForm(formBuilder: FormWrapper, current: Settings): FormWrapper {
        return formBuilder.group({
            idFormat: [current && current.idFormat],
            database: SettingsDatabaseForm.getForm(formBuilder.formBuilder, current && current.database),
            certificateStore: CertificateStoreForm.getForm(formBuilder.formBuilder, current && current.certificateStore),
            customSettings: CustomSettingsForm.getForm(formBuilder.formBuilder, current && current.customSettings),
            agents: SettingsAgentsForm.getForm(formBuilder, current && current.agents).form,
        });
    }
}
