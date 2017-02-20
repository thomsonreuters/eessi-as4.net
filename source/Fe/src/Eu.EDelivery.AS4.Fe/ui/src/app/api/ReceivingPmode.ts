/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ReceivingProcessingMode } from "./ReceivingProcessingMode";
import { IPmode } from './Pmode.interface';

export class ReceivingPmode implements IPmode {
	type: number;
	name: string;
	hash: string;
	pmode: ReceivingProcessingMode;

	static FIELD_type: string = 'type';
	static FIELD_name: string = 'name';
	static FIELD_pmode: string = 'pmode';

	static getForm(formBuilder: FormBuilder, current: ReceivingPmode): FormGroup {
		return formBuilder.group({
			[this.FIELD_type]: [current && current.type],
			[this.FIELD_name]: [current && current.name],
			[this.FIELD_pmode]: ReceivingProcessingMode.getForm(formBuilder, current && current.pmode),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceivingPmode) {
		form.get(this.FIELD_type).reset({ value: current && current.type, disabled: !!!current });
		form.get(this.FIELD_name).reset({ value: current && current.name, disabled: !!!current });
		ReceivingProcessingMode.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_pmode), current && current.pmode);
		form.updateValueAndValidity();
	}
}
