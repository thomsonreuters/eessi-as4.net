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
			[this.FIELD_isEnabled]: [!!(current && current.isEnabled)],
			[this.FIELD_tlsVersion]: [(current == null || current.tlsVersion == null) ? 3 : current.tlsVersion],
			[this.FIELD_clientCertificateReference]: ClientCertificateReference.getForm(formBuilder, current && current.clientCertificateReference),
		});
		TlsConfiguration.setupForm(form);
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: TlsConfiguration) {
		form.get(this.FIELD_isEnabled).reset({ value: current && current.isEnabled, disabled: !!!current });
		form.get(this.FIELD_tlsVersion).reset({ value: current && current.isEnabled, disabled: !!!current || !current.isEnabled });
		ClientCertificateReference.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_clientCertificateReference), current && current.clientCertificateReference);
	}
	static setupForm(form: FormGroup) {
		this.processEnabled(form);
		form.get(TlsConfiguration.FIELD_isEnabled).valueChanges.subscribe(() => this.processEnabled(form));
	}
	static processEnabled(form: FormGroup) {
		let isEnabled = form.get(TlsConfiguration.FIELD_isEnabled).value;
		if (isEnabled) {
			form.get(TlsConfiguration.FIELD_tlsVersion).enable();
			form.get(TlsConfiguration.FIELD_clientCertificateReference).enable();
		}
		else {
			form.get(TlsConfiguration.FIELD_tlsVersion).disable();
			form.get(TlsConfiguration.FIELD_clientCertificateReference).disable();
		}
	}
}
