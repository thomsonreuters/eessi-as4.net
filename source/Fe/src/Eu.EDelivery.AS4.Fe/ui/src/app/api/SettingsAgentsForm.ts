import { SettingsAgentForm } from './SettingsAgentForm';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ReceptionAwarenessForm } from './ReceptionAwarenessForm';
import { SettingsAgents } from './SettingsAgents';

export class SettingsAgentsForm {
    public static getForm(formBuilder: FormBuilder, current: SettingsAgents): FormGroup {
        return formBuilder.group({
            submitAgents: formBuilder.array(!!!(current && current.submitAgents) ? [] : current.submitAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
            receiveAgents: formBuilder.array(!!!(current && current.receiveAgents) ? [] : current.receiveAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
            sendAgents: formBuilder.array(!!!(current && current.sendAgents) ? [] : current.sendAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
            deliverAgents: formBuilder.array(!!!(current && current.deliverAgents) ? [] : current.deliverAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
            notifyAgents: formBuilder.array(!!!(current && current.notifyAgents) ? [] : current.notifyAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))),
            receptionAwarenessAgent: SettingsAgentForm.getForm(formBuilder, current && current.receptionAwarenessAgent),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SettingsAgents) {

        form.removeControl('submitAgents');
        form.addControl('submitAgents', formBuilder.array(!!!(current && current.submitAgents) ? [] : current.submitAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))));
        form.removeControl('receiveAgents');
        form.addControl('receiveAgents', formBuilder.array(!!!(current && current.receiveAgents) ? [] : current.receiveAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))));
        form.removeControl('sendAgents');
        form.addControl('sendAgents', formBuilder.array(!!!(current && current.sendAgents) ? [] : current.sendAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))));
        form.removeControl('deliverAgents');
        form.addControl('deliverAgents', formBuilder.array(!!!(current && current.deliverAgents) ? [] : current.deliverAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))));
        form.removeControl('notifyAgents');
        form.addControl('notifyAgents', formBuilder.array(!!!(current && current.notifyAgents) ? [] : current.notifyAgents.map(item => SettingsAgentForm.getForm(formBuilder, item))));
        form.removeControl('receptionAwarenessAgent');
        form.addControl('receptionAwarenessAgent', SettingsAgentForm.getForm(formBuilder, current && current.receptionAwarenessAgent));
    }
}
