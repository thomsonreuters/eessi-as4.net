import { ItemType } from './ItemType';
import { SendReceiptHandling } from './SendReceiptHandling';
import { FormWrapper } from './../common/form.service';
import { MethodForm } from './MethodForm';

export class SendReceiptHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: SendReceiptHandling, path: string, runtime: ItemType[]): FormWrapper {
        let form = formBuilder
            .group({
                [SendReceiptHandling.FIELD_verifyNRR]: [formBuilder.createFieldValue(current, SendReceiptHandling.FIELD_verifyNRR, path, null, runtime)],
                [SendReceiptHandling.FIELD_notifyMessageProducer]: [formBuilder.createFieldValue(current, SendReceiptHandling.FIELD_notifyMessageProducer, path, null, runtime)],
                [SendReceiptHandling.FIELD_notifyMethod]: MethodForm.getForm(formBuilder.subForm(SendReceiptHandling.FIELD_notifyMethod), current && current.notifyMethod, `${path}.${SendReceiptHandling.FIELD_notifyMethod}`, runtime).form})
            .onChange<boolean>(SendReceiptHandling.FIELD_notifyMessageProducer, (value, wrapper) => {
                if (!!value) {
                    wrapper.enable([SendReceiptHandling.FIELD_notifyMessageProducer]);
                } else {
                    wrapper.disable([SendReceiptHandling.FIELD_notifyMessageProducer]);
                }})
            .triggerHandler(SendReceiptHandling.FIELD_notifyMessageProducer, current && current.notifyMessageProducer);
        return form;
    }
}