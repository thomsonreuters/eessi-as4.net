/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class PartyId {
	id: string;
	type: string;

	static FIELD_id: string = 'id';	
	static FIELD_type: string = 'type';	

	static getForm(formBuilder: FormBuilder, current: PartyId): FormGroup {
		return formBuilder.group({
			id: [current && current.id],
			type: [current && current.type],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: PartyId) {
		form.removeControl('id');
		form.addControl('id', formBuilder.control(current && current.id));
		form.removeControl('type');
		form.addControl('type', formBuilder.control(current && current.type));

	}
}
