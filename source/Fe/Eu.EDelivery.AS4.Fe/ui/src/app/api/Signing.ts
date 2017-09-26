/* tslint:disable */
import { Validators } from '@angular/forms';
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Signing {
	isEnabled: boolean;
	keyReferenceMethod: number;
	signingCertificateInformation: {
		certificateFindType: number,
		certificateFindValue: string
	};
	algorithm: string;
	hashFunction: string;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_signingCertificateInformation: string = 'signingCertificateInformation';
	static FIELD_keyReferenceMethod: string = 'keyReferenceMethod';
	static FIELD_algorithm: string = 'algorithm';
	static FIELD_hashFunction: string = 'hashFunction';
}
