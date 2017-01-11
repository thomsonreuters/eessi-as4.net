/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';

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
		return formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
			algorithm: [current && current.algorithm],
			publicKeyFindType: [current && current.publicKeyFindType],
			publicKeyFindValue: [current && current.publicKeyFindValue],
			keyTransport: [current && current.keyTransport],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Encryption) {
		form.get(this.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current });
		form.get(this.FIELD_algorithm).reset({ value: current && current.algorithm, disabled: !!!current });
		form.get(this.FIELD_publicKeyFindType).reset({ value: current && current.publicKeyFindType, disabled: !!!current });
		form.get(this.FIELD_publicKeyFindValue).reset({ value: current && current.publicKeyFindType, disabled: !!!current });
		form.get(this.FIELD_keyTransport).reset({ value: current && current.keyTransport, disabled: !!!current });
	}
}
