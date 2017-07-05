import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { Deliver } from './Deliver';
import { MethodForm } from './MethodForm';
import { FormWrapper } from './../common/form.service';

export class DeliverForm {
    public static getForm(formBuilder: FormWrapper, current: Deliver): FormWrapper {
        let form = formBuilder
            .group({
                isEnabled: [{ value: !!(current && current.isEnabled), disabled: !!current }],
                payloadReferenceMethod: MethodForm.getForm(formBuilder.subForm(Deliver.FIELD_payloadReferenceMethod), current && current.payloadReferenceMethod).form,
                deliverMethod: MethodForm.getForm(formBuilder.subForm(Deliver.FIELD_deliverMethod), current && current.deliverMethod).form,
            })
            .onChange<boolean>(Deliver.FIELD_isEnabled, (value, wrapper) => {
                let payload = wrapper.form.get(Deliver.FIELD_payloadReferenceMethod);
                if (value) {
                    wrapper.enable([Deliver.FIELD_isEnabled]);
                } else {
                    wrapper.disable([Deliver.FIELD_isEnabled]);
                }
            });
        return form;
    }
}
