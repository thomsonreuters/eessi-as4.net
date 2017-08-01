import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ReceptionAwareness } from './ReceptionAwareness';
import { ItemType } from './ItemType';

export class ReceptionAwarenessForm {
    public static getForm(formBuilder: FormWrapper, current: ReceptionAwareness, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                [ReceptionAwareness.FIELD_isEnabled]: [formBuilder.createFieldValue(current, ReceptionAwareness.FIELD_isEnabled, path, false, runtime), Validators.required],
                [ReceptionAwareness.FIELD_retryCount]: [formBuilder.createFieldValue(current, ReceptionAwareness.FIELD_retryCount, path, 5, runtime), Validators.required],
                [ReceptionAwareness.FIELD_retryInterval]: [formBuilder.createFieldValue(current, ReceptionAwareness.FIELD_retryInterval, path, 0, runtime), Validators.required],
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
