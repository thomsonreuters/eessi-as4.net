import { FormWrapper } from './../common/form.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Receivehandling } from './Receivehandling';
import { MethodForm } from './MethodForm';

export class ReceivehandlingForm {
    public static getForm(formBuilder: FormWrapper, current: Receivehandling): FormWrapper {
        let form = formBuilder
            .group({
                notifyMessageConsumer: [!!(current && current.notifyMessageConsumer)],
                notifyMethod: MethodForm.getForm(formBuilder.subForm(Receivehandling.FIELD_notifyMethod), current && current.notifyMethod).form,
            })
            .onChange<boolean>(Receivehandling.FIELD_notifyMessageConsumer, (value, wrapper) => {
                if (value) {
                    wrapper.enable([Receivehandling.FIELD_notifyMessageConsumer]);
                } else {
                    wrapper.disable([Receivehandling.FIELD_notifyMessageConsumer]);
                }
            });
        return form;
    }
}
