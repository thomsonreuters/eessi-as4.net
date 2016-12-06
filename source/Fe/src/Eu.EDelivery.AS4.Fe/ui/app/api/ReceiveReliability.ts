/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { DuplicateElimination } from "./DuplicateElimination";

export class ReceiveReliability {
	duplicateElimination: DuplicateElimination;

	static FIELD_duplicateElimination: string = 'duplicateElimination';

	static getForm(formBuilder: FormBuilder, current: ReceiveReliability): FormGroup {
		return formBuilder.group({
			duplicateElimination: DuplicateElimination.getForm(formBuilder, current && current.duplicateElimination),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: ReceiveReliability) {
		DuplicateElimination.patchFormArrays(formBuilder, <FormGroup>form.controls['duplicateElimination'], current && current.duplicateElimination);
	}
}
