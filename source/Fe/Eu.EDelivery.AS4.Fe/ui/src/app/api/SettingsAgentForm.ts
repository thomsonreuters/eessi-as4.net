import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { SettingsAgent } from './SettingsAgent';
import { ReceiverForm } from './ReceiverForm';
import { TransformerForm } from './TransformerForm';
import { StepsForm } from './StepsForm';

export class SettingsAgentForm {
    public static getForm(formBuilder: FormWrapper, current: SettingsAgent | undefined = undefined): FormWrapper {
        return formBuilder.group({
            [SettingsAgent.FIELD_name]: [current && current.name],
            [SettingsAgent.FIELD_receiver]: ReceiverForm.getForm(formBuilder.subForm(SettingsAgent.FIELD_receiver), current && current.receiver, formBuilder.injector).form,
            [SettingsAgent.FIELD_transformer]: TransformerForm.getForm(formBuilder.subForm(SettingsAgent.FIELD_transformer), current && current.transformer, formBuilder.injector).form,
            [SettingsAgent.FIELD_stepConfiguration]: StepsForm.getForm(formBuilder.subForm(SettingsAgent.FIELD_stepConfiguration), current && current.stepConfiguration).form
        });
    }
}
