import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { FormWrapper } from './../common/form.service';
import { Receivehandling } from './Receivehandling';
import { MethodForm } from './MethodForm';

export class ReceivehandlingForm {
    public static getForm(formBuilder: FormWrapper, current: Receivehandling, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                notifyMessageConsumer: [!!(current && current.notifyMessageConsumer)],
                notifyMethod: MethodForm.getForm(formBuilder.subForm(Receivehandling.FIELD_notifyMethod), current && current.notifyMethod, `${path}.${Receivehandling.FIELD_notifyMethod}`, runtime).form,
            })
            .onChange<boolean>(Receivehandling.FIELD_notifyMessageConsumer, (value, wrapper) => {
                if (value) {
                    wrapper.enable([Receivehandling.FIELD_notifyMessageConsumer]);
                } else {
                    wrapper.disable([Receivehandling.FIELD_notifyMessageConsumer]);
                }
            })
            .triggerHandler(Receivehandling.FIELD_notifyMessageConsumer, current && current.notifyMessageConsumer);
        return form;
    }
}
