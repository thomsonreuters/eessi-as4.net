/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { SigningVerification } from "./SigningVerification";
import { Decryption } from "./Decryption";

export class ReceiveSecurity {
	signingVerification: SigningVerification = new SigningVerification();
	decryption: Decryption = new Decryption();

	static FIELD_signingVerification: string = 'signingVerification';
	static FIELD_decryption: string = 'decryption';
}
