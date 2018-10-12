import { FormBuilder, FormGroup } from '@angular/forms';

import { SettingsSubmit } from './SettingsSubmit';

export class SettingsSubmitForm {
  public static getForm(
    formBuilder: FormBuilder,
    current: SettingsSubmit
  ): FormGroup {
    return formBuilder.group({
      payloadRetrievalPath: [current && current.payloadRetrievalPath]
    });
  }
}
