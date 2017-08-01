import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { SendHandling } from './SendHandling';
import { MethodForm } from './MethodForm';

export class SendHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: SendHandling, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                [SendHandling.FIELD_notifyMessageProducer]: [formBuilder.createFieldValue(current, SendHandling.FIELD_notifyMessageProducer, path, null, runtime)],
                [SendHandling.FIELD_notifyMethod]: MethodForm.getForm(formBuilder.subForm(SendHandling.FIELD_notifyMethod), current && current.notifyMethod, `${path}.${SendHandling.FIELD_notifyMethod}`, runtime).form,
            })
            .onChange<boolean>(SendHandling.FIELD_notifyMessageProducer, (value, wrapper) => {
                if (!!value) {
                    wrapper.enable([SendHandling.FIELD_notifyMessageProducer]);
                } else {
                    wrapper.disable([SendHandling.FIELD_notifyMessageProducer]);
                }
            })
            .triggerHandler(SendHandling.FIELD_notifyMessageProducer, current && current.notifyMessageProducer);
        return form;
    }
}
