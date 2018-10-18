/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { SettingsPullSend } from './SettingsPullSend';

export class SettingsPullSendForm {
  public static getForm(
    formBuilder: FormBuilder,
    current: SettingsPullSend
  ): FormGroup {
    return formBuilder.group({
      authorizationMapPath: [current && current.authorizationMapPath]
    });
  }
}
