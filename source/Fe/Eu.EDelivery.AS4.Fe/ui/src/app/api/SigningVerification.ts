/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class SigningVerification {
	signature: number;
	allowUnknownRootCertificate: boolean;

	static FIELD_signature: string = 'signature';	
	static FIELD_allowUnknownRootCertificate: string = 'allowUnknownRootCertificate';
}
