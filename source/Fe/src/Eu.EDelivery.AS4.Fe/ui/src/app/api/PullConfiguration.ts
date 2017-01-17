/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class PullConfiguration {
	subChannel: string;

	static FIELD_subChannel: string = 'subChannel';

	static getForm(formBuilder: FormBuilder, current: PullConfiguration): FormGroup {
		return formBuilder.group({
			subChannel: [current && current.subChannel],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: PullConfiguration) {
		form.get(this.FIELD_subChannel).reset({ value: current && current.subChannel, disabled: !!!current });
	}
}
