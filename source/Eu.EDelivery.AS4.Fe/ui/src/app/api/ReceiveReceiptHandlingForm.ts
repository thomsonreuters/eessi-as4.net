import { ItemType } from './ItemType';
import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';
import { FormWrapper } from './../common/form.service';

export class ReceiveReceiptHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveReceiptHandling, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder
            .group({
                [ReceiveReceiptHandling.FIELD_useNRRFormat]: [formBuilder.createFieldValue(current, ReceiveReceiptHandling.FIELD_useNRRFormat, path, null, runtime)]
            });
    }
}
