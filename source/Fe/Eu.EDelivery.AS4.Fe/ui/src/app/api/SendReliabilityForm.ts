import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { SendReliability } from './SendReliability';
import { ReceptionAwarenessForm } from './ReceptionAwarenessForm';

export class SendReliabilityForm {
    public static getForm(formBuilder: FormWrapper, current: SendReliability): FormWrapper {
        return formBuilder.group({
            receptionAwareness: ReceptionAwarenessForm.getForm(formBuilder.subForm(SendReliability.FIELD_receptionAwareness), current && current.receptionAwareness).form,
        });
    }
}
