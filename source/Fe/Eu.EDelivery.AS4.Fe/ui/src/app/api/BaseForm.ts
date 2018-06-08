import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Base } from './Base';
import { CertificateStoreForm } from './CertificateStoreForm';
import { SettingsRetryReliabilityForm } from './SettingsRetryReliabilityForm';

export class BaseForm {
  public static getForm(formBuilder: FormBuilder, current: Base): FormGroup {
    return formBuilder.group({
      [Base.FIELD_idFormat]: [current && current.idFormat, Validators.required],
      [Base.FIELD_retentionPeriod]: [
        current && current.retentionPeriod,
        Validators.min(0)
      ],
      [Base.FIELD_retryReliability]: SettingsRetryReliabilityForm.getForm(
        formBuilder,
        current && current.retryReliability
      ),
      [Base.FIELD_certificateStore]: CertificateStoreForm.getForm(
        formBuilder,
        current && current.certificateStore
      )
    });
  }
}
