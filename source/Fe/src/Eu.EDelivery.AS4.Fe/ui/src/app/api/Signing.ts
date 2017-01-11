/* tslint:disable */
import { Validators } from '@angular/forms';
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Signing {
	isEnabled: boolean;
	privateKeyFindValue: string;
	privateKeyFindType: number;
	keyReferenceMethod: number;
	algorithm: string;
	hashFunction: string;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_privateKeyFindValue: string = 'privateKeyFindValue';
	static FIELD_privateKeyFindType: string = 'privateKeyFindType';
	static FIELD_keyReferenceMethod: string = 'keyReferenceMethod';
	static FIELD_algorithm: string = 'algorithm';
	static FIELD_hashFunction: string = 'hashFunction';

	static getForm(formBuilder: FormBuilder, current: Signing): FormGroup {
		let form = formBuilder.group({
			[this.FIELD_isEnabled]: [!!(current && current.isEnabled)],
			[this.FIELD_privateKeyFindValue]: [current && current.privateKeyFindValue, Validators.required],
			[this.FIELD_privateKeyFindType]: [current && current.privateKeyFindType, Validators.required],
			[this.FIELD_keyReferenceMethod]: [current && current.keyReferenceMethod, Validators.required],
			[this.FIELD_algorithm]: [(current == null || current.algorithm == null) ? 'http://www.w3.org/2001/04/xmldsig-more#rsa-sha256' : current.algorithm, Validators.required],
			[this.FIELD_hashFunction]: [(current == null || current.hashFunction == null) ? 'http://www.w3.org/2001/04/xmlenc#sha256' : current.hashFunction, Validators.required],
		});
		setTimeout(() => this.setupForm(form));
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Signing) {
		form.get(this.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current });
		form.get(this.FIELD_privateKeyFindValue).reset({ value: current && current.privateKeyFindValue, disabled: !!!current });
		form.get(this.FIELD_privateKeyFindType).reset({ value: current && current.privateKeyFindType, disabled: !!!current });
		form.get(this.FIELD_keyReferenceMethod).reset({ value: current && current.keyReferenceMethod, disabled: !!!current });
		form.get(this.FIELD_algorithm).reset({ value: current && current.algorithm, disabled: !!!current });
		form.get(this.FIELD_hashFunction).reset({ value: current && current.hashFunction, disabled: !!!current });
		this.setupForm(form);
	}

	static setupForm(formGroup: FormGroup) {
		let fields = Object.keys(this).filter(key => key.startsWith('FIELD_') && !key.endsWith('isEnabled')).map(field => formGroup.get(this[field]));
		let isEnabled = formGroup.get(this.FIELD_isEnabled);
		let toggle = (value: boolean) => {
			if (value) fields.forEach(field => field.enable());
			else fields.forEach(field => field.disable());
		}
		toggle(isEnabled.value);
		isEnabled.valueChanges.subscribe(result => toggle(result));
	}
}
