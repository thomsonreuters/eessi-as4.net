/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class LoginModel {
	username: string;
	password: string;

	static FIELD_username: string = 'username';	
	static FIELD_password: string = 'password';	

	static getForm(formBuilder: FormBuilder, current: LoginModel): FormGroup {
		return formBuilder.group({
			username: [current && current.username],
			password: [current && current.password],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: LoginModel) {
	}
}
