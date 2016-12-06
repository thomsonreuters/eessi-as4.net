/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class DuplicateElimination {
	isEnabled: boolean;

	static FIELD_isEnabled: string = 'isEnabled';	

	static getForm(formBuilder: FormBuilder, current: DuplicateElimination): FormGroup {
		return formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: DuplicateElimination) {
	}
}
