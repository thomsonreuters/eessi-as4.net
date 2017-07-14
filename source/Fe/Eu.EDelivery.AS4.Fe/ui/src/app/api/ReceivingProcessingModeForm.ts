import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ReceivingProcessingMode } from './ReceivingProcessingMode';
import { ReplyHandlingSettingForm } from './ReplyHandlingSettingForm';
import { ReceiveReliabilityForm } from './ReceiveReliabilityForm';
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
            [ReceivingProcessingMode.FIELD_replyHandling]: ReplyHandlingSettingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_replyHandling), current && current.replyHandling).form,
            [ReceivingProcessingMode.FIELD_exceptionHandling]: ReceivehandlingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_exceptionHandling), current && current.exceptionHandling).form,
            [ReceivingProcessingMode.FIELD_security]: ReceiveSecurityForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_security), current && current.security).form,
            [ReceivingProcessingMode.FIELD_messagePackaging]: MessagePackagingForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_messagePackaging), current && current.messagePackaging).form,
            [ReceivingProcessingMode.FIELD_deliver]: DeliverForm.getForm(formBuilder.subForm(ReceivingProcessingMode.FIELD_deliver), current && current.deliver).form,
        });
    }
}
