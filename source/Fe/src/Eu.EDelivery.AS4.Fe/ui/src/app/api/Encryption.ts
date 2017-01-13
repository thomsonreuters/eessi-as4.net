/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';
import { Validators } from '@angular/forms';
import { thumbPrintValidation } from '../validators/thumbprintValidator';

export class Encryption {
	isEnabled: boolean;
	algorithm: string;
	publicKeyFindType: number;
	publicKeyFindValue: string;
	keyTransport: string;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_algorithm: string = 'algorithm';
	static FIELD_publicKeyFindType: string = 'publicKeyFindType';
	static FIELD_publicKeyFindValue: string = 'publicKeyFindValue';
	static FIELD_keyTransport: string = 'keyTransport';

	static getForm(formBuilder: FormBuilder, current: Encryption): FormGroup {
		let form = formBuilder.group({
			isEnabled: [!!(current && current.isEnabled), Validators.required],
			algorithm: [current && current.algorithm, Validators.required],
			publicKeyFindType: [current && current.publicKeyFindType, Validators.required],
			publicKeyFindValue: [current && current.publicKeyFindValue, Validators.required],
			keyTransport: [current && current.keyTransport, Validators.required],
		});
		this.setupForm(form);
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Encryption) {
		form.get(this.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current });
		form.get(this.FIELD_algorithm).reset({ value: current && current.algorithm, disabled: !!!current || !current.isEnabled });
		form.get(this.FIELD_publicKeyFindType).reset({ value: current && current.publicKeyFindType, disabled: !!!current || !current.isEnabled });
		form.get(this.FIELD_publicKeyFindValue).reset({ value: current && current.publicKeyFindValue, disabled: !!!current || !current.isEnabled });
		form.get(this.FIELD_keyTransport).reset({ value: current && current.keyTransport, disabled: !!!current || !current.isEnabled });
	}
	static setupForm(form: FormGroup) {
		let fields = [this.FIELD_algorithm, this.FIELD_keyTransport, this.FIELD_publicKeyFindType, this.FIELD_publicKeyFindValue];
		let value = form.get(this.FIELD_publicKeyFindValue);
		let isEnabled = form.get(this.FIELD_isEnabled);

		form.get(this.FIELD_publicKeyFindType)
			.valueChanges
			.subscribe((result: number) => {
				value.clearValidators();
				if (+result === 0) value.setValidators([Validators.required, thumbPrintValidation]);
				else value.setValidators(Validators.required);
				value.updateValueAndValidity();
			});

		isEnabled
			.valueChanges
			.map(result => !!result)
			.subscribe(result => {
				if (result) fields.forEach(el => form.get(el).enable());
				else fields.forEach(el => form.get(el).disable());
			});
	}
}
