import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ReceivingProcessingMode } from './ReceivingProcessingMode';
import { ReceiveReliabilityForm } from './ReceiveReliabilityForm';
import { ReceiveReceiptHandlingForm } from './ReceiveReceiptHandlingForm';
import { ReceiveErrorHandlingForm } from './ReceiveErrorHandlingForm';
import { ReceivehandlingForm } from './ReceivehandlingForm';
import { ReceiveSecurityForm } from './ReceiveSecurityForm';
import { MessagePackagingForm } from './MessagePackagingForm';
import { DeliverForm } from './DeliverForm';
import { FormWrapper } from './../common/form.service';

export class ReceivingProcessingModeForm {
    public static getForm(formBuilder: FormWrapper, current: ReceivingProcessingMode): FormWrapper {
        return formBuilder.group({
            [ReceivingProcessingMode.FIELD_id]: [current && current.id, Validators.required],
            [ReceivingProcessingMode.FIELD_mep]: [(current == null || current.mep == null) ? 0 : current.mep, Validators.required],
            [ReceivingProcessingMode.FIELD_mepBinding]: [(current == null || current.mepBinding == null) ? 1 : current.mepBinding, Validators.required],
            [ReceivingProcessingMode.FIELD_reliability]: ReceiveReliabilityForm.getForm(formBuilder.formBuilder, current && current.reliability),
            [ReceivingProcessingMode.FIELD_receiptHandling]: ReceiveReceiptHandlingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_receiptHandling), current && current.receiptHandling).form,
            [ReceivingProcessingMode.FIELD_errorHandling]: ReceiveErrorHandlingForm.getForm(formBuilder.formBuilder, current && current.errorHandling),
            [ReceivingProcessingMode.FIELD_exceptionHandling]: ReceivehandlingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_exceptionHandling), current && current.exceptionHandling).form,
            [ReceivingProcessingMode.FIELD_security]: ReceiveSecurityForm.getForm(formBuilder.formBuilder, current && current.security),
            [ReceivingProcessingMode.FIELD_messagePackaging]: MessagePackagingForm.getForm(formBuilder.formBuilder, current && current.messagePackaging),
            [ReceivingProcessingMode.FIELD_deliver]: DeliverForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_deliver), current && current.deliver).form,
        });
    }
}
