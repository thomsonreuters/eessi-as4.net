import { ReceiveErrorHandling } from './ReceiveErrorHandling';
import { FormWrapper } from './../common/form.service';

export class ReceiveErrorHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveErrorHandling): FormWrapper {
        return formBuilder
            .group({
                [ReceiveErrorHandling.FIELD_useSoapFault]: [current && current.useSoapFault],
                [ReceiveErrorHandling.FIELD_responseHttpCode]: [current && current.responseHttpCode]
            });
    }
}
