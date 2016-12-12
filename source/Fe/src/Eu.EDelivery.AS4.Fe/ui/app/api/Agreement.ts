/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Agreement {
	value: string;
	refType: string;
	pModeId: string;

	static FIELD_value: string = 'value';	
	static FIELD_refType: string = 'refType';	
	static FIELD_pModeId: string = 'pModeId';	

	static getForm(formBuilder: FormBuilder, current: Agreement): FormGroup {
		return formBuilder.group({
			value: [current && current.value],
			refType: [current && current.refType],
			pModeId: [current && current.pModeId],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Agreement) {
		form.removeControl('value');
		form.addControl('value', formBuilder.control(current && current.value));
		form.removeControl('refType');
		form.addControl('refType', formBuilder.control(current && current.refType));
		form.removeControl('pModeId');
		form.addControl('pModeId', formBuilder.control(current && current.pModeId));

	}
}
