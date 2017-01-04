/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class SettingsDatabase {
	provider: string;
	connectionString: string;

	static FIELD_provider: string = 'provider';	
	static FIELD_connectionString: string = 'connectionString';	

	static getForm(formBuilder: FormBuilder, current: SettingsDatabase): FormGroup {
		return formBuilder.group({
			provider: [current && current.provider],
			connectionString: [current && current.connectionString],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SettingsDatabase) {
		form.removeControl('provider');
		form.addControl('provider', formBuilder.control(current && current.provider));
		form.removeControl('connectionString');
		form.addControl('connectionString', formBuilder.control(current && current.connectionString));

	}
}
