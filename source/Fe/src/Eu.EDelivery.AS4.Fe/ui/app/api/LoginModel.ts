/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

export class LoginModel {
	username: string;
	password: string;

	static getForm(formBuilder: FormBuilder, current: LoginModel): FormGroup {
		return formBuilder.group({
				username: [current && current.username],
				password: [current && current.password],
		});
	}
}
