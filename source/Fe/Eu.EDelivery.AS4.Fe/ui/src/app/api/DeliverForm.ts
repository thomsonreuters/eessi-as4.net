import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Deliver } from './Deliver';
import { ItemType } from './ItemType';
import { MethodForm } from './MethodForm';
import { FormWrapper } from './../common/form.service';

export class DeliverForm {
    public static getForm(formBuilder: FormWrapper, current: Deliver, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                isEnabled: [{ value: !!(current && current.isEnabled), disabled: !!current }],
                payloadReferenceMethod: MethodForm.getForm(formBuilder.subForm(Deliver.FIELD_payloadReferenceMethod), current && current.payloadReferenceMethod, `${path}.${Deliver.FIELD_payloadReferenceMethod}`, runtime).form,
                deliverMethod: MethodForm.getForm(formBuilder.subForm(Deliver.FIELD_deliverMethod), current && current.deliverMethod, `${path}.${Deliver.FIELD_deliverMethod}`, runtime).form,
            })
            .onChange<boolean>(Deliver.FIELD_isEnabled, (value, wrapper) => {
                let payload = wrapper.form.get(Deliver.FIELD_payloadReferenceMethod);
                if (value) {
                    wrapper.enable([Deliver.FIELD_isEnabled]);
                } else {
                    wrapper.disable([Deliver.FIELD_isEnabled]);
                }
            })
            .triggerHandler(Deliver.FIELD_isEnabled, current && current.isEnabled);
        return form;
    }
}
