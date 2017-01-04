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
		form.removeControl(this.FIELD_isEnabled);
		form.addControl(this.FIELD_isEnabled, formBuilder.control(current && current.isEnabled));
		form.removeControl(this.FIELD_privateKeyFindValue);
		form.addControl(this.FIELD_privateKeyFindValue, formBuilder.control(current && current.privateKeyFindValue));
		form.removeControl(this.FIELD_privateKeyFindType);
		form.addControl(this.FIELD_privateKeyFindType, formBuilder.control(current && current.privateKeyFindType));
		form.removeControl(this.FIELD_keyReferenceMethod);
		form.addControl(this.FIELD_keyReferenceMethod, formBuilder.control(current && current.keyReferenceMethod));
		form.removeControl(this.FIELD_algorithm);
		form.addControl(this.FIELD_algorithm, formBuilder.control(current && current.algorithm));
		form.removeControl(this.FIELD_hashFunction);
		form.addControl(this.FIELD_hashFunction, formBuilder.control(current && current.hashFunction));
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
