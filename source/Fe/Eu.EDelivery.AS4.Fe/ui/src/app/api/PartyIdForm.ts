import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PartyId } from './PartyId';

export class PartyIdForm {
    public static getForm(formBuilder: FormBuilder, current: PartyId | undefined = undefined): FormGroup {
        return formBuilder.group({
            id: [current && current.id],
            type: [current && current.type],
        });
    }
}
