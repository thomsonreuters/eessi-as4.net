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

	static getForm(formBuilder: FormBuilder, current: TlsConfiguration): FormGroup {
		return formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
			tlsVersion: [current && current.tlsVersion],
			clientCertificateReference: ClientCertificateReference.getForm(formBuilder, current && current.clientCertificateReference),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: TlsConfiguration) {
		form.removeControl('isEnabled');
		form.addControl('isEnabled', formBuilder.control(current && current.isEnabled));
		form.removeControl('tlsVersion');
		form.addControl('tlsVersion', formBuilder.control(current && current.tlsVersion));

		form.removeControl('clientCertificateReference');
		form.addControl('clientCertificateReference', ClientCertificateReference.getForm(formBuilder, current && current.clientCertificateReference));
	}
}
