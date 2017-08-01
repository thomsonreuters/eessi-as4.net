import { ItemType } from './ItemType';
import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';
import { FormWrapper } from './../common/form.service';

export class ReceiveReceiptHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveReceiptHandling, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder
            .group({
                [ReceiveReceiptHandling.FIELD_useNNRFormat]: [formBuilder.createFieldValue(current, ReceiveReceiptHandling.FIELD_useNNRFormat, path, null, runtime)]
            });
    }
}
