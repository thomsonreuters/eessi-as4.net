import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { SettingsAgentForm } from './SettingsAgentForm';
import { FormWrapper } from './../common/form.service';
import { ReceptionAwarenessForm } from './ReceptionAwarenessForm';
import { SettingsAgents } from './SettingsAgents';

export class SettingsAgentsForm {
    public static getForm(formBuilder: FormWrapper, current: SettingsAgents): FormWrapper {
        return formBuilder
            .group({
                submitAgents: formBuilder.formBuilder.array(!!!(current && current.submitAgents) ? [] : current.submitAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                outboundProcessingAgents: formBuilder.formBuilder.array(!!!(current && current.outboundProcessingAgents) ? [] : current.outboundProcessingAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                sendAgents: formBuilder.formBuilder.array(!!!(current && current.sendAgents) ? [] : current.sendAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                receiveAgents: formBuilder.formBuilder.array(!!!(current && current.receiveAgents) ? [] : current.receiveAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                deliverAgents: formBuilder.formBuilder.array(!!!(current && current.deliverAgents) ? [] : current.deliverAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                notifyAgents: formBuilder.formBuilder.array(!!!(current && current.notifyAgents) ? [] : current.notifyAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                notifyConsumerAgents: formBuilder.formBuilder.array(!!!(current && current.notifyConsumerAgents) ? [] : current.notifyConsumerAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                notifyProducerAgents: formBuilder.formBuilder.array(!!!(current && current.notifyProducerAgents) ? [] : current.notifyProducerAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                receptionAwarenessAgent: SettingsAgentForm.getForm(formBuilder, current && current.receptionAwarenessAgent),
                pullReceiveAgents: formBuilder.formBuilder.array(!!!(current && current.pullReceiveAgents) ? [] : current.pullReceiveAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
                pullSendAgents: formBuilder.formBuilder.array(!!!(current && current.pullSendAgents) ? [] : current.pullSendAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
            });
    }
}
