import { FormBuilder, FormGroup, FormGroupDirective, Validators } from '@angular/forms';

import { MethodForm } from './MethodForm';
import { ItemType } from './ItemType';
import { MessageHandling, Deliver, Forward } from './MessageHandling';
import { FormWrapper } from './../common/form.service';

/* tslint:disable */
export class MessageHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: MessageHandling, runtime: ItemType[], path: string = 'messagehandling'): FormWrapper {
        return formBuilder
            .group({
                [MessageHandling.FIELD_messageHandlingType]: [!!!current || !!!current.messageHandlingType ? 1 : current.messageHandlingType, Validators.required],
                [MessageHandling.FIELD_item]: []
            })
            .onChange<number>(MessageHandling.FIELD_messageHandlingType, (_current, wrapper) => {
                wrapper.form.removeControl(MessageHandling.FIELD_item);
                if (_current === 1) {
                    wrapper.form.setControl(MessageHandling.FIELD_item, wrapper.formBuilder.group({
                        [Deliver.FIELD_isEnabled]: [!!!(current && !!current.item && (<Deliver>current.item).isEnabled) ? false : true, Validators.required],
                        [Deliver.FIELD_payloadReferenceMethod]: MethodForm.getForm(formBuilder.subForm(Deliver.FIELD_payloadReferenceMethod), !!!current || !!!current.item ? null : (<Deliver>current.item).payloadReferenceMethod, `${path}.${Deliver.FIELD_payloadReferenceMethod}`, runtime).form,
                        [Deliver.FIELD_deliverMethod]: MethodForm.getForm(formBuilder.subForm(Deliver.FIELD_deliverMethod), !!!current || !!!current.item ? null : (<Deliver>current.item).deliverMethod, `${path}.${Deliver.FIELD_deliverMethod}`, runtime).form
                    }));
                }
                else if (_current === 2) {
                    wrapper.form.setControl(MessageHandling.FIELD_item, wrapper.formBuilder.group({
                        [Forward.FIELD_sendingPmode]: [!!!current || !!!current.item ? null : (<Forward>current.item).sendingPMode, Validators.required]
                    }));
                }

                if (!!!current) {
                    wrapper.form.get(MessageHandling.FIELD_item)!.disable({ emitEvent: false });
                }
            })
            .triggerHandler(MessageHandling.FIELD_messageHandlingType, current && current.messageHandlingType);
    }

}
