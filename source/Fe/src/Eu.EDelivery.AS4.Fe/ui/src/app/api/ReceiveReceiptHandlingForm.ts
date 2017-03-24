import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceiveReceiptHandling } from './ReceiveReceiptHandling';

export class ReceiveReceiptHandlingForm {
    public static getForm(formBuilder: FormBuilder, current: ReceiveReceiptHandling): FormGroup {
        let form = formBuilder.group({
            [ReceiveReceiptHandling.FIELD_useNNRFormat]: [!!(current && current.useNNRFormat), Validators.required],
            [ReceiveReceiptHandling.FIELD_replyPattern]: [(current == null || current.replyPattern == null) ? 0 : current.replyPattern, Validators.required],
            [ReceiveReceiptHandling.FIELD_callbackUrl]: [current && current.callbackUrl],
            [ReceiveReceiptHandling.FIELD_sendingPMode]: [current && current.sendingPMode],
        });
        ReceiveReceiptHandlingForm.setupForm(form);
        return form;
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveReceiptHandling) {
        form.get(ReceiveReceiptHandling.FIELD_useNNRFormat).reset({ value: current && current.useNNRFormat, disabled: !!!current && form.parent.disabled });
        form.get(ReceiveReceiptHandling.FIELD_replyPattern).reset({ value: current && current.replyPattern, disabled: !!!current && form.parent.disabled });
        form.get(ReceiveReceiptHandling.FIELD_callbackUrl).reset({ value: current && current.callbackUrl, disabled: !!!current && form.parent.disabled });
        form.get(ReceiveReceiptHandling.FIELD_sendingPMode).reset({ value: current && current.sendingPMode, disabled: !!!current && form.parent.disabled });
        form.get(ReceiveReceiptHandling.FIELD_sendingPMode).reset({ value: current && current.sendingPMode, disabled: !!!current && form.parent.disabled });
    }

    public static setupForm(form: FormGroup) {
        let patternChanges = form.get(ReceiveReceiptHandling.FIELD_replyPattern);

        patternChanges.valueChanges.subscribe((result) => this.processCallbackUrl(form));
        this.processCallbackUrl(form);
    }

    public static processCallbackUrl(form: FormGroup) {
        let callbackUrl = form.get(ReceiveReceiptHandling.FIELD_callbackUrl);
        let replyPattern = form.get(ReceiveReceiptHandling.FIELD_replyPattern).value;

        if (+replyPattern === 1) // Callback
        {
            callbackUrl.setValidators(Validators.required);
            callbackUrl.enable();
        }
        else {
            callbackUrl.clearValidators();
            callbackUrl.disable();
        }
    }
}
