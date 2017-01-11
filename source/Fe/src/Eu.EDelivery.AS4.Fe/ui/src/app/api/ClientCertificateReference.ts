import { Validators } from '@angular/forms';
/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class ClientCertificateReference {
	clientCertificateFindType: number;
	clientCertificateFindValue: string;

	static FIELD_clientCertificateFindType: string = 'clientCertificateFindType';
	static FIELD_clientCertificateFindValue: string = 'clientCertificateFindValue';

	static getForm(formBuilder: FormBuilder, current: ClientCertificateReference): FormGroup {
		return formBuilder.group({
			clientCertificateFindType: [current && current.clientCertificateFindType, Validators.required],
			clientCertificateFindValue: [current && current.clientCertificateFindValue, Validators.required],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ClientCertificateReference) {
		form.get(this.FIELD_clientCertificateFindType).reset({ value: current && current.clientCertificateFindType, disabled: !!!current });
		form.get(this.FIELD_clientCertificateFindValue).reset({ value: current && current.clientCertificateFindValue, disabled: !!!current });
	}
}
