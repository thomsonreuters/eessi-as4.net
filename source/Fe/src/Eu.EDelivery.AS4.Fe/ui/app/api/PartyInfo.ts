/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Party } from "./Party";

export class PartyInfo {
	fromParty: Party;
	toParty: Party;

	static FIELD_fromParty: string = 'fromParty';
	static FIELD_toParty: string = 'toParty';

	static getForm(formBuilder: FormBuilder, current: PartyInfo): FormGroup {
		return formBuilder.group({
			fromParty: Party.getForm(formBuilder, current && current.fromParty),
			toParty: Party.getForm(formBuilder, current && current.toParty),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: PartyInfo) {

		form.removeControl('fromParty');
		form.addControl('fromParty', Party.getForm(formBuilder, current && current.fromParty));
		form.removeControl('toParty');
		form.addControl('toParty', Party.getForm(formBuilder, current && current.toParty));
	}
}
