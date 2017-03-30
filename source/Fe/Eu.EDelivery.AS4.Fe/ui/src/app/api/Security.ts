/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Signing } from "./Signing";
import { Encryption } from "./Encryption";

export class Security {
	signing: Signing = new Signing();
	encryption: Encryption = new Encryption();

	static FIELD_signing: string = 'signing';
	static FIELD_encryption: string = 'encryption';
}
