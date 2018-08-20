import { ItemType } from './ItemType';
import { ReceiveErrorHandling } from './ReceiveErrorHandling';
import { FormWrapper } from './../common/form.service';

export class ReceiveErrorHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveErrorHandling, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder
            .group({
                [ReceiveErrorHandling.FIELD_responseHttpCode]: [formBuilder.createFieldValue(current, ReceiveErrorHandling.FIELD_responseHttpCode, path, null, runtime)]
            });
    }
}
