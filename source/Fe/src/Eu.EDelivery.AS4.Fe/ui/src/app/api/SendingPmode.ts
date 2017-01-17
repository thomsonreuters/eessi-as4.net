/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SendingProcessingMode } from "./SendingProcessingMode";
import { IPmode } from './Pmode.interface';

export class SendingPmode implements IPmode {
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
		form.get(this.FIELD_type).reset({ value: current && current.type, disabled: !!!current });
		form.get(this.FIELD_name).reset({ value: current && current.name, disabled: !!!current });
		SendingProcessingMode.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_pmode), current && current.pmode);
		form.markAsPristine();
	}
}
