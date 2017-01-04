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
		let form = formBuilder.group({
			isEnabled: [!!(current && current.isEnabled)],
			tlsVersion: [(current == null || current.tlsVersion == null) ? 3 : current.tlsVersion],
			clientCertificateReference: ClientCertificateReference.getForm(formBuilder, current && current.clientCertificateReference),
		});
		TlsConfiguration.setupForm(form);
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: TlsConfiguration) {
		form.removeControl('isEnabled');
		form.addControl('isEnabled', formBuilder.control(current && current.isEnabled));
		form.removeControl('tlsVersion');
		form.addControl('tlsVersion', formBuilder.control(current && current.tlsVersion));

		form.removeControl('clientCertificateReference');
		form.addControl('clientCertificateReference', ClientCertificateReference.getForm(formBuilder, current && current.clientCertificateReference));
		TlsConfiguration.setupForm(form);
	}

	static setupForm(form: FormGroup) {
		this.processEnabled(form);
		form.get(TlsConfiguration.FIELD_isEnabled).valueChanges.subscribe(() => this.processEnabled(form));
	}

	static processEnabled(form: FormGroup) {
		let isEnabled = form.get(TlsConfiguration.FIELD_isEnabled).value;
		if (isEnabled) {
			setTimeout(() => {
				form.get(TlsConfiguration.FIELD_tlsVersion).enable();
				form.get(TlsConfiguration.FIELD_clientCertificateReference).enable();
			});
		}
		else {
			setTimeout(() => {
				form.get(TlsConfiguration.FIELD_tlsVersion).disable();
				form.get(TlsConfiguration.FIELD_clientCertificateReference).disable();
			});
		}
	}
}
