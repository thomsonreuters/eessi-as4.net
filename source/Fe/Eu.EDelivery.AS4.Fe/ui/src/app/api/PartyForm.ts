import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Party } from './Party';
import { PartyIdForm } from './PartyIdForm';

export class PartyForm {
    public static getForm(formBuilder: FormBuilder, current: Party): FormGroup {
        return formBuilder.group({
            role: [current && current.role],
            partyIds: formBuilder.array(!!!(current && current.partyIds) ? [PartyIdForm.getForm(formBuilder)] : current.partyIds.map(item => PartyIdForm.getForm(formBuilder, item))),
        });
    }
}
