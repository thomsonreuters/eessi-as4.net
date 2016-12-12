/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { CertificateStore } from "./CertificateStore";

export class BaseSettings {
	idFormat: string;
	certificateStore: CertificateStore;

	static FIELD_idFormat: string = 'idFormat';	
	static FIELD_certificateStore: string = 'certificateStore';

	static getForm(formBuilder: FormBuilder, current: BaseSettings): FormGroup {
		return formBuilder.group({
			idFormat: [current && current.idFormat],
			certificateStore: CertificateStore.getForm(formBuilder, current && current.certificateStore),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: BaseSettings) {
		form.removeControl('idFormat');
		form.addControl('idFormat', formBuilder.control(current && current.idFormat));

		form.removeControl('certificateStore');
		form.addControl('certificateStore', CertificateStore.getForm(formBuilder, current && current.certificateStore));
	}
}
