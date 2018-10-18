import { FormWrapper } from './../common/form.service';
import { CertificateStoreForm } from './CertificateStoreForm';
import { CustomSettingsForm } from './CustomSettingsForm';
import { Settings } from './Settings';
import { SettingsAgentsForm } from './SettingsAgentsForm';
import { SettingsDatabaseForm } from './SettingsDatabaseForm';
import { SettingsPullSendForm } from './SettingsPullSendForm';

export class SettingsForm {
  public static getForm(
    formBuilder: FormWrapper,
    current: Settings
  ): FormWrapper {
    return formBuilder.group({
      idFormat: [current && current.idFormat],
      retentionPeriod: [current && current.retentionPeriod],
      pullSend: SettingsPullSendForm.getForm(
        formBuilder.formBuilder,
        current && current.pullSend
      ),
      database: SettingsDatabaseForm.getForm(
        formBuilder.formBuilder,
        current && current.database
      ),
      certificateStore: CertificateStoreForm.getForm(
        formBuilder.formBuilder,
        current && current.certificateStore
      ),
      customSettings: CustomSettingsForm.getForm(
        formBuilder.formBuilder,
        current && current.customSettings
      ),
      agents: SettingsAgentsForm.getForm(formBuilder, current && current.agents)
        .form
    });
  }
}
