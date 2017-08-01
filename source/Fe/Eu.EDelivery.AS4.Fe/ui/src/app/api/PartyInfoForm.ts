import { FormBuilder, FormGroup, Validators } from '@angular/forms';

import { FormWrapper } from './../common/form.service';
import { ItemType } from './ItemType';
import { PartyInfo } from './PartyInfo';
import { PartyForm } from './PartyForm';

export class PartyInfoForm {
    public static getForm(formBuilder: FormWrapper, current: PartyInfo, path: string, runtime: ItemType[]): FormWrapper {
        return formBuilder.group({
            fromParty: PartyForm.getForm(formBuilder.subForm(PartyInfo.FIELD_fromParty), current && current.fromParty, `${path}.${PartyInfo.FIELD_fromParty}`, runtime).form,
            toParty: PartyForm.getForm(formBuilder.subForm(PartyInfo.FIELD_toParty), current && current.toParty, `${path}.${PartyInfo.FIELD_toParty}`, runtime).form,
        });
    }
}
