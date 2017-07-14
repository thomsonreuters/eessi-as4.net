import { DynamicDiscoveryForm } from './DynamicDiscoveryForm';
import { FormBuilder, FormGroup } from '@angular/forms';

import { PushConfigurationForm } from './PushConfigurationForm';
import { SendingProcessingMode } from './SendingProcessingMode';
import { SendReliabilityForm } from './SendReliabilityForm';
import { SendMessagePackagingForm } from './SendMessagePackagingForm';
import { SecurityForm } from './SecurityForm'; 7
import { SendHandlingForm } from './SendHandlingForm';
import { FormWrapper } from './../common/form.service';

export class SendingProcessingModeForm {
    public static getForm(formBuilder: FormWrapper, current: SendingProcessingMode): FormWrapper {
        let previousIsPushEnabled: boolean | null = null;
        return formBuilder
            .group({
                id: [current && current.id],
                allowOverride: [!!(current && current.allowOverride)],
                mep: [(current == null || current.mep == null) ? 0 : current.mep],
                mepBinding: [(current == null || current.mepBinding == null) ? 1 : current.mepBinding],
                pushConfiguration: PushConfigurationForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_pushConfiguration), current && current.pushConfiguration).form,
                [SendingProcessingMode.FIELD_dynamicDiscovery]: DynamicDiscoveryForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_dynamicDiscovery), current && current.dynamicDiscovery).form,
                reliability: SendReliabilityForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_reliability), current && current.reliability).form,
                receiptHandling: SendHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_receiptHandling), current && current.receiptHandling).form,
                errorHandling: SendHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_errorHandling), current && current.errorHandling).form,
                exceptionHandling: SendHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_exceptionHandling), current && current.exceptionHandling).form,
                security: SecurityForm.getForm(formBuilder.subForm('security'), current && current.security).form,
                messagePackaging: SendMessagePackagingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_messagePackaging), current && current.messagePackaging).form
            });
    }
}
