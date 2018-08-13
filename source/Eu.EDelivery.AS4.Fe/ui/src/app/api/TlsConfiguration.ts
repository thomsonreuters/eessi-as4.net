/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ClientCertificateInformation } from "./ClientCertificateInformation";

export class TlsConfiguration {
	isEnabled: boolean;
	tlsVersion: number;
	clientCertificateInformation: ClientCertificateInformation;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_tlsVersion: string = 'tlsVersion';
	static FIELD_clientCertificateInformation: string = 'clientCertificateInformation';	
}
