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
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ReceiveReliability) {
		form.get(this.FIELD_duplicateElimination).reset({ value: current && current.duplicateElimination, disabled: !!!current && form.parent.disabled });
		DuplicateElimination.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_duplicateElimination), current && current.duplicateElimination);
	}
}
