import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';
import { FormWrapper } from './../common/form.service';

export class ReceiveReceiptHandlingForm {
    public static getForm(formBuilder: FormWrapper, current: ReceiveReceiptHandling): FormWrapper {
        let form = formBuilder
            .group({
                [ReceiveReceiptHandling.FIELD_useNNRFormat]: [!!(current && current.useNNRFormat), Validators.required],
                [ReceiveReceiptHandling.FIELD_replyPattern]: [(current == null || current.replyPattern == null) ? 0 : current.replyPattern, Validators.required],
                [ReceiveReceiptHandling.FIELD_sendingPMode]: [current && current.sendingPMode],
            });
        return form;
    }
}
