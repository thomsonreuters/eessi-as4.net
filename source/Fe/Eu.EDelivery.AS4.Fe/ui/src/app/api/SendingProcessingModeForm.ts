import { FormBuilder, FormGroup } from '@angular/forms';

import { PushConfigurationForm } from './PushConfigurationForm';
import { SendingProcessingMode } from './SendingProcessingMode';
import { SendReliabilityForm } from './SendReliabilityForm';
import { SendMessagePackagingForm } from './SendMessagePackagingForm';
import { SecurityForm } from './SecurityForm';
import { SendHandlingForm } from './SendHandlingForm';
import { PullConfigurationForm } from './PullConfigurationForm';
import { FormWrapper } from './../common/form.service';

export class SendingProcessingModeForm {
    public static getForm(formBuilder: FormWrapper, current: SendingProcessingMode): FormWrapper {
        return formBuilder.group({
            id: [current && current.id],
            allowOverride: [!!(current && current.allowOverride)],
            mep: [(current == null || current.mep == null) ? 0 : current.mep],
            mepBinding: [(current == null || current.mepBinding == null) ? 1 : current.mepBinding],
            pushConfiguration: PushConfigurationForm.getForm(formBuilder.formBuilder, current && current.pushConfiguration),
            pullConfiguration: PullConfigurationForm.getForm(formBuilder.formBuilder, current && current.pullConfiguration),
            reliability: SendReliabilityForm.getForm(formBuilder.formBuilder, current && current.reliability),
            receiptHandling: SendHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_receiptHandling), current && current.receiptHandling).form,
            errorHandling: SendHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_errorHandling), current && current.errorHandling).form,
            exceptionHandling: SendHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_exceptionHandling), current && current.exceptionHandling).form,
            security: SecurityForm.getForm(formBuilder.subForm('security'), current && current.security).form,
            messagePackaging: SendMessagePackagingForm.getForm(formBuilder.formBuilder, current && current.messagePackaging),
        });
    }
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendingProcessingMode) {
        form.get(SendingProcessingMode.FIELD_id).reset({ value: current && current.id, disabled: !!!current || form.parent.disabled });
        form.get(SendingProcessingMode.FIELD_allowOverride).reset({ value: current && current.allowOverride, disabled: !!!current || form.parent.disabled });
        form.get(SendingProcessingMode.FIELD_mep).reset({ value: current && current.mep, disabled: !!!current || form.parent.disabled });
        form.get(SendingProcessingMode.FIELD_mepBinding).reset({ value: current && current.mepBinding, disabled: !!!current || form.parent.disabled });

        PushConfigurationForm.patchForm(formBuilder, <FormGroup>form.get(SendingProcessingMode.FIELD_pushConfiguration), current && current.pushConfiguration);
        PullConfigurationForm.patchForm(formBuilder, <FormGroup>form.get(SendingProcessingMode.FIELD_pullConfiguration), current && current.pullConfiguration);
        SendReliabilityForm.patchForm(formBuilder, <FormGroup>form.get(SendingProcessingMode.FIELD_reliability), current && current.reliability);
        SendHandlingForm.patchForm(formBuilder, <FormGroup>form.get(SendingProcessingMode.FIELD_receiptHandling), current && current.receiptHandling);
        SendHandlingForm.patchForm(formBuilder, <FormGroup>form.get(SendingProcessingMode.FIELD_errorHandling), current && current.errorHandling);
        SendHandlingForm.patchForm(formBuilder, <FormGroup>form.get(SendingProcessingMode.FIELD_exceptionHandling), current && current.exceptionHandling);
        SecurityForm.patchForm(formBuilder, <FormGroup>form.get(SendingProcessingMode.FIELD_security), current && current.security);
        SendMessagePackagingForm.patchForm(formBuilder, <FormGroup>form.get(SendingProcessingMode.FIELD_messagePackaging), current && current.messagePackaging);
    }
}