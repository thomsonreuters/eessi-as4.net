import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';
import { FormWrapper } from './../common/form.service';

export class ReceiveReceiptHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveReceiptHandling): FormWrapper {
        return formBuilder
            .group({
                [ReceiveReceiptHandling.FIELD_useNNRFormat]: [current && current.useNNRFormat]
            });
    }
}
