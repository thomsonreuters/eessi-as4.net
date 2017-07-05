import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceiveErrorHandling } from './ReceiveErrorHandling';

export class ReceiveErrorHandlingForm {
    public static getForm(formBuilder: FormBuilder, current: ReceiveErrorHandling): FormGroup {
        return formBuilder.group({
            useSoapFault: [!!(current && current.useSoapFault)],
            replyPattern: [(current == null || current.replyPattern == null) ? 0 : current.replyPattern],
            responseHttpCode: [!!!current ? null : current.responseHttpCode],
            sendingPMode: [current && current.sendingPMode],
        });
    }
}
