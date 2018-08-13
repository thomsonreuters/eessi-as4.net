import { FormBuilder, FormGroup } from '@angular/forms';

import { SettingsRetryReliability } from './SettingsRetryReliability';

export class SettingsRetryReliabilityForm {
  public static getForm(
    formBuilder: FormBuilder,
    current: SettingsRetryReliability
  ): FormGroup {
    return formBuilder.group({
      pollingInterval: [current && current.pollingInterval]
    });
  }
}
