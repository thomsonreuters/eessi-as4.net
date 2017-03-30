import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceiveErrorHandling } from './ReceiveErrorHandling';

export class ReceiveErrorHandlingForm {
    public static getForm(formBuilder: FormBuilder, current: ReceiveErrorHandling): FormGroup {
        return formBuilder.group({
            useSoapFault: [!!(current && current.useSoapFault)],
            replyPattern: [(current == null || current.replyPattern == null) ? 0 : current.replyPattern],
            callbackUrl: [current && current.callbackUrl],
            responseHttpCode: [(current == null || current.responseHttpCode == null) ? '200' : current.responseHttpCode],
            sendingPMode: [current && current.sendingPMode],
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveErrorHandling) {
        form.get(ReceiveErrorHandling.FIELD_useSoapFault).reset({ value: current && current.useSoapFault, disabled: !!!current && form.parent.disabled });
        form.get(ReceiveErrorHandling.FIELD_replyPattern).reset({ value: current && current.replyPattern, disabled: !!!current && form.parent.disabled });
        form.get(ReceiveErrorHandling.FIELD_callbackUrl).reset({ value: current && current.callbackUrl, disabled: !!!current && form.parent.disabled });
        form.get(ReceiveErrorHandling.FIELD_responseHttpCode).reset({ value: current && current.responseHttpCode, disabled: !!!current && form.parent.disabled });
        form.get(ReceiveErrorHandling.FIELD_sendingPMode).reset({ value: current && current.sendingPMode, disabled: !!!current && form.parent.disabled });
    }
}
