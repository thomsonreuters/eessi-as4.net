/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ClientCertificateReference } from "./ClientCertificateReference";

export class TlsConfiguration {
	isEnabled: boolean;
	tlsVersion: number;
	clientCertificateReference: ClientCertificateReference;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_tlsVersion: string = 'tlsVersion';
	static FIELD_clientCertificateReference: string = 'clientCertificateReference';	
}
