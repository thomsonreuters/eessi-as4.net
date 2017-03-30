/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class LoginModel {
	username: string;
	password: string;

	static FIELD_username: string = 'username';	
	static FIELD_password: string = 'password';
}
