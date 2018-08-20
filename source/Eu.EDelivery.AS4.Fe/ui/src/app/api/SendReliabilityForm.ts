import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { SendReliability } from './SendReliability';
import { ReceptionAwarenessForm } from './ReceptionAwarenessForm';
import { ItemType } from './ItemType';

export class SendReliabilityForm {
    public static getForm(formBuilder: FormWrapper, current: SendReliability, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            [SendReliability.FIELD_receptionAwareness]: ReceptionAwarenessForm.getForm(formBuilder.subForm(SendReliability.FIELD_receptionAwareness), current && current.receptionAwareness, `${path}.${SendReliability.FIELD_receptionAwareness}`, runtime).form,
        });
    }
}
