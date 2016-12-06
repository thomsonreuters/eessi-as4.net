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
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: SendingPmode) {
		SendingProcessingMode.patchFormArrays(formBuilder, <FormGroup>form.controls['pmode'], current && current.pmode);
	}
}
