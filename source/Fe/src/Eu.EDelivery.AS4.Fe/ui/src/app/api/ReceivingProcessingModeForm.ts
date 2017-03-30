import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceivingProcessingMode } from './ReceivingProcessingMode';
import { ReceiveReliabilityForm } from './ReceiveReliabilityForm';
import { ReceiveReceiptHandlingForm } from './ReceiveReceiptHandlingForm';
import { ReceiveErrorHandlingForm } from './ReceiveErrorHandlingForm';
import { ReceivehandlingForm } from './ReceivehandlingForm';
import { ReceiveSecurityForm } from './ReceiveSecurityForm';
import { MessagePackagingForm } from './MessagePackagingForm';
import { DeliverForm } from './DeliverForm';

export class ReceivingProcessingModeForm {
    public static getForm(formBuilder: FormBuilder, current: ReceivingProcessingMode): FormGroup {
        return formBuilder.group({
            [ReceivingProcessingMode.FIELD_id]: [current && current.id, Validators.required],
            [ReceivingProcessingMode.FIELD_mep]: [(current == null || current.mep == null) ? 0 : current.mep, Validators.required],
            [ReceivingProcessingMode.FIELD_mepBinding]: [(current == null || current.mepBinding == null) ? 1 : current.mepBinding, Validators.required],
            [ReceivingProcessingMode.FIELD_reliability]: ReceiveReliabilityForm.getForm(formBuilder, current && current.reliability),
            [ReceivingProcessingMode.FIELD_receiptHandling]: ReceiveReceiptHandlingForm.getForm(formBuilder, current && current.receiptHandling),
            [ReceivingProcessingMode.FIELD_errorHandling]: ReceiveErrorHandlingForm.getForm(formBuilder, current && current.errorHandling),
            [ReceivingProcessingMode.FIELD_exceptionHandling]: ReceivehandlingForm.getForm(formBuilder, current && current.exceptionHandling),
            [ReceivingProcessingMode.FIELD_security]: ReceiveSecurityForm.getForm(formBuilder, current && current.security),
            [ReceivingProcessingMode.FIELD_messagePackaging]: MessagePackagingForm.getForm(formBuilder, current && current.messagePackaging),
            [ReceivingProcessingMode.FIELD_deliver]: DeliverForm.getForm(formBuilder, current && current.deliver),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceivingProcessingMode) {
        form.get(ReceivingProcessingMode.FIELD_id).reset({ value: current && current.id, disabled: !!!current || form.parent.disabled });
        form.get(ReceivingProcessingMode.FIELD_mep).reset({ value: current && current.mep, disabled: !!!current || form.parent.disabled });
        form.get(ReceivingProcessingMode.FIELD_mepBinding).reset({ value: current && current.mepBinding, disabled: !!!current || form.parent.disabled });
        ReceiveReliabilityForm.patchForm(formBuilder, <FormGroup>form.get(ReceivingProcessingMode.FIELD_reliability), current && current.reliability);
        ReceiveReceiptHandlingForm.patchForm(formBuilder, <FormGroup>form.get(ReceivingProcessingMode.FIELD_receiptHandling), current && current.receiptHandling);
        ReceiveErrorHandlingForm.patchForm(formBuilder, <FormGroup>form.get(ReceivingProcessingMode.FIELD_errorHandling), current && current.errorHandling);
        ReceivehandlingForm.patchForm(formBuilder, <FormGroup>form.get(ReceivingProcessingMode.FIELD_exceptionHandling), current && current.exceptionHandling);
        ReceiveSecurityForm.patchForm(formBuilder, <FormGroup>form.get(ReceivingProcessingMode.FIELD_security), current && current.security);
        MessagePackagingForm.patchForm(formBuilder, <FormGroup>form.get(ReceivingProcessingMode.FIELD_messagePackaging), current && current.messagePackaging);
        DeliverForm.patchForm(formBuilder, <FormGroup>form.get(ReceivingProcessingMode.FIELD_deliver), current && current.deliver);
    }
}
