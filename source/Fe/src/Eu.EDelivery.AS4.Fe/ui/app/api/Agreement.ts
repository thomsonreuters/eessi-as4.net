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
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Agreement) {
	}
}
