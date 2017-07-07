import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ReceptionAwareness } from './ReceptionAwareness';

export class ReceptionAwarenessForm {
    public static getForm(formBuilder: FormWrapper, current: ReceptionAwareness): FormWrapper {
        let form = formBuilder
            .group({
                [ReceptionAwareness.FIELD_isEnabled]: [!!(current && current.isEnabled), Validators.required],
                [ReceptionAwareness.FIELD_retryCount]: [(current == null || current.retryCount == null) ? 0 : current.retryCount, Validators.required],
                [ReceptionAwareness.FIELD_retryInterval]: [(current == null || current.retryInterval == null) ? 0 : current.retryInterval, Validators.required],
            })
            .onChange<boolean>(ReceptionAwareness.FIELD_isEnabled, (result, wrapper) => {
                let isEnabled = wrapper.form.get(ReceptionAwareness.FIELD_isEnabled)!.value;
                if (isEnabled) {
                    wrapper.form.get(ReceptionAwareness.FIELD_retryCount)!.enable();
                    wrapper.form.get(ReceptionAwareness.FIELD_retryInterval)!.enable();
                } else {
                    wrapper.form.get(ReceptionAwareness.FIELD_retryCount)!.disable();
                    wrapper.form.get(ReceptionAwareness.FIELD_retryInterval)!.disable();
                }
            })
            .triggerHandler(ReceptionAwareness.FIELD_isEnabled, current && current.isEnabled);
        return form;
    }
}
