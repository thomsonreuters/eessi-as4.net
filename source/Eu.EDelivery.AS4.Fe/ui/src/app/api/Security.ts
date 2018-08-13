/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Signing } from "./Signing";
import { Encryption } from "./Encryption";
import { SigningVerification } from './SigningVerification';

export class Security {
	signing: Signing = new Signing();
	signingVerification: SigningVerification = new SigningVerification();
	encryption: Encryption = new Encryption();

	static FIELD_signing: string = 'signing';
	static FIELD_signingVerification: string = 'signingVerification';
	static FIELD_encryption: string = 'encryption';
}
