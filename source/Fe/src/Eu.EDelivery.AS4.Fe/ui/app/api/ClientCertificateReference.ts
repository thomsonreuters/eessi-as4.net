/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class ClientCertificateReference {
	clientCertificateFindType: number;
	clientCertificateFindValue: string;

	static FIELD_clientCertificateFindType: string = 'clientCertificateFindType';	
	static FIELD_clientCertificateFindValue: string = 'clientCertificateFindValue';	

	static getForm(formBuilder: FormBuilder, current: ClientCertificateReference): FormGroup {
		return formBuilder.group({
			clientCertificateFindType: [current && current.clientCertificateFindType],
			clientCertificateFindValue: [current && current.clientCertificateFindValue],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: ClientCertificateReference) {
	}
}
