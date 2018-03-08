import { SendingPmode } from './SendingPmode';
import { FormBuilder, FormGroup } from '@angular/forms';

import { PushConfigurationForm } from './PushConfigurationForm';
import { SendingProcessingMode } from './SendingProcessingMode';
import { SendReliabilityForm } from './SendReliabilityForm';
import { SendMessagePackagingForm } from './SendMessagePackagingForm';
import { SecurityForm } from './SecurityForm';
import { SendHandlingForm } from './SendHandlingForm';
import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { DynamicDiscoveryForm } from './DynamicDiscoveryForm';
import { jsonAccessor } from '../common/jsonAccessor';
import { SendReceiptHandlingForm } from './SendReceiptHandlingForm';

export class SendingProcessingModeForm {
    public static getForm(formBuilder: FormWrapper, current: SendingProcessingMode, runtime: ItemType[], path: string = 'sendingprocessingmode'): FormWrapper {
        let previousIsPushEnabled: boolean | null = null;
        return formBuilder
            .group({
                id: [current && current.id],
                allowOverride: [!!(current && current.allowOverride)],
                mep: [(current == null || current.mep == null) ? 0 : current.mep],
                [SendingProcessingMode.FIELD_mepBinding]: [formBuilder.createFieldValue(current, SendingProcessingMode.FIELD_mepBinding, path, 0, runtime)],
                [SendingProcessingMode.FIELD_pushConfiguration]: PushConfigurationForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_pushConfiguration), current && current.pushConfiguration, runtime, `${path}.${SendingProcessingMode.FIELD_pushConfiguration}`).form,
                [SendingProcessingMode.FIELD_dynamicDiscovery]: DynamicDiscoveryForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_dynamicDiscovery), current && current.dynamicDiscovery, runtime, `${path}.${SendingProcessingMode.FIELD_dynamicDiscovery}`).form,
                [SendingProcessingMode.FIELD_reliability]: SendReliabilityForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_reliability), current && current.reliability, `${path}.${SendingProcessingMode.FIELD_reliability}`, runtime).form,
                [SendingProcessingMode.FIELD_receiptHandling]: SendReceiptHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_receiptHandling), current && current.receiptHandling, `${path}.${SendingProcessingMode.FIELD_receiptHandling}`, runtime).form,
                [SendingProcessingMode.FIELD_errorHandling]: SendHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_errorHandling), current && current.errorHandling, `${path}.${SendingProcessingMode.FIELD_errorHandling}`, runtime).form,
                [SendingProcessingMode.FIELD_exceptionHandling]: SendHandlingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_exceptionHandling), current && current.exceptionHandling, `${path}.${SendingProcessingMode.FIELD_exceptionHandling}`, runtime).form,
                [SendingProcessingMode.FIELD_security]: SecurityForm.getForm(formBuilder.subForm('security'), current && current.security, `${path}.${SendingProcessingMode.FIELD_security}`, runtime).form,
                [SendingProcessingMode.FIELD_messagePackaging]: SendMessagePackagingForm.getForm(formBuilder.subForm(SendingProcessingMode.FIELD_messagePackaging), current && current.messagePackaging, `${path}.${SendingProcessingMode.FIELD_messagePackaging}`, runtime).form
            })
            .onChange<number>(SendingProcessingMode.FIELD_mepBinding, (value, wrapper) => {
                const isDynamicEnabled = wrapper.form.parent.get(SendingPmode.FIELD_isDynamicDiscoveryEnabled)!.value;
                if (!!!current || +value === 1) {
                    wrapper.form.get(SendingProcessingMode.FIELD_pushConfiguration)!.disable();
                    wrapper.form.get(SendingProcessingMode.FIELD_dynamicDiscovery)!.disable();
                } else {
                    if (!isDynamicEnabled) {
                        wrapper.form.get(SendingProcessingMode.FIELD_pushConfiguration)!.enable();
                    } else {
                        wrapper.form.get(SendingProcessingMode.FIELD_dynamicDiscovery)!.enable();
                    }
                }
            })
            .triggerHandler(SendingProcessingMode.FIELD_mepBinding, current && current.mepBinding);
    }
}
