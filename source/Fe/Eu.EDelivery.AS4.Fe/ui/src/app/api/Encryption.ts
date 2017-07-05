/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, FormControl } from '@angular/forms';
import { Validators } from '@angular/forms';
import { KeyEncryption } from './KeyEncryption';

export class Encryption {
	isEnabled: boolean;
	algorithm: string;
	algorithmKeySize: number;
	publicKeyType: number;
	publicKeyInformation: PublicKeyFindCriteria | PublicKeyCertificate | null;
	keyTransport: KeyEncryption = new KeyEncryption();

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_algorithm: string = 'algorithm';
	static FIELD_algorithmKeySize: string = 'algorithmKeySize';
	static FIELD_publicKeyType: string = 'publicKeyType';
	static FIELD_publicKeyInformation: string = 'publicKeyInformation';
	// static FIELD_publicKeyFindType: string = 'publicKeyFindType';
	// static FIELD_publicKeyFindValue: string = 'publicKeyFindValue';
	static FIELD_keyTransport: string = 'keyTransport';

	static defaultAlgorithm: string = 'http://www.w3.org/2001/04/xmldsig-more#rsa-sha256';
}


export class PublicKeyFindCriteria {
	public publicKeyFindType: number;
	public publicKeyFindValue: string;
}

export class PublicKeyCertificate {
	public certificate: string;
}