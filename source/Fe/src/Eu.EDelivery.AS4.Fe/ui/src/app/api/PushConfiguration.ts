/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Protocol } from "./Protocol";
import { TlsConfiguration } from "./TlsConfiguration";

export class PushConfiguration {
	protocol: Protocol;
	tlsConfiguration: TlsConfiguration;

	static FIELD_protocol: string = 'protocol';
	static FIELD_tlsConfiguration: string = 'tlsConfiguration';

	static getForm(formBuilder: FormBuilder, current: PushConfiguration): FormGroup {
		return formBuilder.group({
			protocol: Protocol.getForm(formBuilder, current && current.protocol),
			tlsConfiguration: TlsConfiguration.getForm(formBuilder, current && current.tlsConfiguration),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: PushConfiguration) {
		Protocol.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_protocol), current && current.protocol);
		TlsConfiguration.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_tlsConfiguration), current && current.tlsConfiguration);
	}
}
