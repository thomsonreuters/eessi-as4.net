/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { PartyId } from "./PartyId";

export class Party {
	role: string;
	partyIds: PartyId[];

	static FIELD_role: string = 'role';
	static FIELD_partyIds: string = 'partyIds';

	static getForm(formBuilder: FormBuilder, current: Party): FormGroup {
		return formBuilder.group({
			role: [current && current.role],
			partyIds: formBuilder.array(!!!(current && current.partyIds) ? [] : current.partyIds.map(item => PartyId.getForm(formBuilder, item))),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Party) {
		form.get(this.FIELD_role).reset({ value: current && current.role, disabled: !!!current && form.parent.disabled });
		form.removeControl('partyIds');
		form.addControl('partyIds', formBuilder.array(!!!(current && current.partyIds) ? [] : current.partyIds.map(item => PartyId.getForm(formBuilder, item))));
	}
}
