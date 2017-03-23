/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Protocol } from "./Protocol";
import { TlsConfiguration } from "./TlsConfiguration";

export class PushConfiguration {
	protocol: Protocol = new Protocol();
	tlsConfiguration: TlsConfiguration = new TlsConfiguration();

	static FIELD_protocol: string = 'protocol';
	static FIELD_tlsConfiguration: string = 'tlsConfiguration';	
}
