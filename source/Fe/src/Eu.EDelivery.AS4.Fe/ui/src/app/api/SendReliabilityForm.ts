import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { SendReliability } from './SendReliability';
import { ReceptionAwarenessForm } from './ReceptionAwarenessForm';

export class SendReliabilityForm {
    public static getForm(formBuilder: FormBuilder, current: SendReliability): FormGroup {
        return formBuilder.group({
            receptionAwareness: ReceptionAwarenessForm.getForm(formBuilder, current && current.receptionAwareness),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendReliability) {
        ReceptionAwarenessForm.patchForm(formBuilder, <FormGroup>form.get(SendReliability.FIELD_receptionAwareness), current && current.receptionAwareness);
    }
}
