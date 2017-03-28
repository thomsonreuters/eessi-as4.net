import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SettingsAgent } from './SettingsAgent';
import { ReceiverForm } from './ReceiverForm';
import { TransformerForm } from './TransformerForm';
import { StepsForm } from './StepsForm';

export class SettingsAgentForm {
    public static getForm(formBuilder: FormBuilder, current: SettingsAgent): FormGroup {
        return formBuilder.group({
            name: [current && current.name],
            receiver: ReceiverForm.getForm(formBuilder, current && current.receiver),
            transformer: TransformerForm.getForm(formBuilder, current && current.transformer),
            steps: StepsForm.getForm(formBuilder, current && current.steps),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SettingsAgent) {
        form.removeControl('name');
        form.addControl('name', formBuilder.control(current && current.name));

        form.removeControl('receiver');
        form.addControl('receiver', ReceiverForm.getForm(formBuilder, current && current.receiver));
        form.removeControl('transformer');
        form.addControl('transformer', TransformerForm.getForm(formBuilder, current && current.transformer));
        form.removeControl('steps');
        form.addControl('steps', StepsForm.getForm(formBuilder, current && current.steps));
    }
}
