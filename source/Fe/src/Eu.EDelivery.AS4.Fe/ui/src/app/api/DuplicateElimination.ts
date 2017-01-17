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
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: DuplicateElimination) {
		form.get(this.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current && form.parent.disabled });
	}
}
