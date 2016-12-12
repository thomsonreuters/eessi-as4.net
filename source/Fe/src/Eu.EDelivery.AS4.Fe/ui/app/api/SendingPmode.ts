/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SendingProcessingMode } from "./SendingProcessingMode";

export class SendingPmode {
	type: number;
	name: string;
	pmode: SendingProcessingMode;

	static FIELD_type: string = 'type';	
	static FIELD_name: string = 'name';	
	static FIELD_pmode: string = 'pmode';

	static getForm(formBuilder: FormBuilder, current: SendingPmode): FormGroup {
		return formBuilder.group({
			type: [current && current.type],
			name: [current && current.name],
			pmode: SendingProcessingMode.getForm(formBuilder, current && current.pmode),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendingPmode) {
		form.removeControl('type');
		form.addControl('type', formBuilder.control(current && current.type));
		form.removeControl('name');
		form.addControl('name', formBuilder.control(current && current.name));

		form.removeControl('pmode');
		form.addControl('pmode', SendingProcessingMode.getForm(formBuilder, current && current.pmode));
	}
}
