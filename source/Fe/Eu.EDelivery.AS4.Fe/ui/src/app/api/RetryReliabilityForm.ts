import { Validators } from '@angular/forms';

import { FormWrapper } from '../common/form.service';
import { ItemType } from './ItemType';
import { RetryReliability } from './RetryReliability';

export class RetryReliabilityForm {
  public static getForm(
    formBuilder: FormWrapper,
    current: RetryReliability | null,
    path: string,
    runtime: ItemType[]
  ) {
    const form = formBuilder
      .group({
        [RetryReliability.FIELD_isEnabled]: [
          !!!current || !!!current.isEnabled
            ? false
            : current && current.isEnabled
        ],
        [RetryReliability.FIELD_retryCount]: [
          formBuilder.createFieldValue(
            current,
            RetryReliability.FIELD_retryCount,
            path,
            null,
            runtime
          ),
          Validators.required
        ],
        [RetryReliability.FIELD_retryInterval]: [
          formBuilder.createFieldValue(
            current,
            RetryReliability.FIELD_retryInterval,
            path,
            null,
            runtime
          ),
          Validators.required
        ]
      })
      .onChange<boolean>(RetryReliability.FIELD_isEnabled, (field, wrapper) => {
        if (field) {
          wrapper.enable([RetryReliability.FIELD_isEnabled]);
        } else {
          wrapper.disable([RetryReliability.FIELD_isEnabled]);
        }
      })
      .triggerHandler(
        RetryReliability.FIELD_isEnabled,
        current && current.isEnabled
      );

    return form;
  }
}
