import { FormWrapper } from './../common/form.service';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { ItemType } from './ItemType';
import { Party } from './Party';
import { PartyIdForm } from './PartyIdForm';

export class PartyForm {
    public static getForm(formBuilder: FormWrapper, current: Party, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            role: [formBuilder.createFieldValue(current, Party.FIELD_role, path, null, runtime)],
            partyIds: formBuilder.formBuilder.array(!!!(current && current.partyIds) ? [PartyIdForm.getForm(formBuilder.formBuilder)] : current.partyIds.map(item => PartyIdForm.getForm(formBuilder.formBuilder, item))),
        });
    }
}
