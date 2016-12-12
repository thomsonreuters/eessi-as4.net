/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ReceivingProcessingMode } from "./ReceivingProcessingMode";

export class ReceivingPmode {
	type: number;
	name: string;
	pmode: ReceivingProcessingMode;

	static FIELD_type: string = 'type';	
	static FIELD_name: string = 'name';	
	static FIELD_pmode: string = 'pmode';

	static getForm(formBuilder: FormBuilder, current: ReceivingPmode): FormGroup {
		return formBuilder.group({
			type: [current && current.type],
			name: [current && current.name],
			pmode: ReceivingProcessingMode.getForm(formBuilder, current && current.pmode),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceivingPmode) {
		form.removeControl('type');
		form.addControl('type', formBuilder.control(current && current.type));
		form.removeControl('name');
		form.addControl('name', formBuilder.control(current && current.name));

		form.removeControl('pmode');
		form.addControl('pmode', ReceivingProcessingMode.getForm(formBuilder, current && current.pmode));
	}
}
