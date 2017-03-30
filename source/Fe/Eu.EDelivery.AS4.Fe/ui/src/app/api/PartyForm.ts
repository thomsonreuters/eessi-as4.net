import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Party } from './Party';
import { PartyIdForm } from './PartyIdForm';

export class PartyForm {
    public static getForm(formBuilder: FormBuilder, current: Party): FormGroup {
        return formBuilder.group({
            role: [current && current.role],
            partyIds: formBuilder.array(!!!(current && current.partyIds) ? [] : current.partyIds.map(item => PartyIdForm.getForm(formBuilder, item))),
        });
    }
    /// Patch up all the formArray controls
    public static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Party) {
        form.get(Party.FIELD_role).reset({ value: current && current.role, disabled: !!!current && form.parent.disabled });
        form.removeControl('partyIds');
        form.addControl('partyIds', formBuilder.array(!!!(current && current.partyIds) ? [] : current.partyIds.map(item => PartyIdForm.getForm(formBuilder, item))));
    }
}
