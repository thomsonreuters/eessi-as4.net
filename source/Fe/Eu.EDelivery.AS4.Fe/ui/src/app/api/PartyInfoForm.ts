import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { PartyInfo } from './PartyInfo';
import { PartyForm } from './PartyForm';

export class PartyInfoForm {
    public static getForm(formBuilder: FormBuilder, current: PartyInfo): FormGroup {
        return formBuilder.group({
            fromParty: PartyForm.getForm(formBuilder, current && current.fromParty),
            toParty: PartyForm.getForm(formBuilder, current && current.toParty),
        });
    }
}
