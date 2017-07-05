import { FormWrapper } from './../common/form.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SendHandling } from './SendHandling';
import { MethodForm } from './MethodForm';

export class SendHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: SendHandling): FormWrapper {
        let form = formBuilder
            .group({
                [SendHandling.FIELD_notifyMessageProducer]: [!!(current && current.notifyMessageProducer)],
                [SendHandling.FIELD_notifyMethod]: MethodForm.getForm(formBuilder.subForm(SendHandling.FIELD_notifyMethod), current && current.notifyMethod).form,
            })
            .onChange<boolean>(SendHandling.FIELD_notifyMessageProducer, (value, wrapper) => {
                if (!!value) {
                    wrapper.enable([SendHandling.FIELD_notifyMessageProducer]);
                } else {
                    wrapper.disable([SendHandling.FIELD_notifyMessageProducer]);
                }
            });
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendHandling) {
        form.get(SendHandling.FIELD_notifyMessageProducer).reset({ value: current && current.notifyMessageProducer, disabled: !!!current });
        MethodForm.patchForm(formBuilder, <FormGroup>form.get(SendHandling.FIELD_notifyMethod), current && current.notifyMethod, !!!current || !current.notifyMessageProducer);
    }
}
