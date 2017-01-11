/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Service {
	value: string;
	type: string;

	static FIELD_value: string = 'value';
	static FIELD_type: string = 'type';

	static getForm(formBuilder: FormBuilder, current: Service): FormGroup {
		return formBuilder.group({
			value: [current && current.value],
			type: [current && current.type],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Service) {
		form.get(this.FIELD_value).reset({ value: current && current.value, disabled: !!!current && form.parent.disabled });
		form.get(this.FIELD_type).reset({ value: current && current.type, disabled: !!!current && form.parent.disabled });
	}
}
