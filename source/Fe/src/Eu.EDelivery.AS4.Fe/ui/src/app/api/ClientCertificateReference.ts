/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, Validators, FormControl } from '@angular/forms';
import { thumbPrintValidation } from '../validators/thumbprintValidator';

export class ClientCertificateReference {
	clientCertificateFindType: number;
	clientCertificateFindValue: string;

	static FIELD_clientCertificateFindType: string = 'clientCertificateFindType';
	static FIELD_clientCertificateFindValue: string = 'clientCertificateFindValue';

	static getForm(formBuilder: FormBuilder, current: ClientCertificateReference): FormGroup {
		let form = formBuilder.group({
			clientCertificateFindType: [current && current.clientCertificateFindType, Validators.required],
			clientCertificateFindValue: [current && current.clientCertificateFindValue, [Validators.required, thumbPrintValidation]]
		});
		this.setupForm(form);
		return form;
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: ClientCertificateReference) {
		form.get(this.FIELD_clientCertificateFindType).reset({ value: current && current.clientCertificateFindType, disabled: !!!current });
		form.get(this.FIELD_clientCertificateFindValue).reset({ value: current && current.clientCertificateFindValue, disabled: !!!current });
	}
	static setupForm(form: FormGroup) {
		let findValue = form.get(this.FIELD_clientCertificateFindValue);
		form.get(this.FIELD_clientCertificateFindType)
			.valueChanges
			.subscribe((result: number) => {
				findValue.clearValidators();
				if (+result === 0)
					findValue.setValidators([Validators.required, thumbPrintValidation]);
				else
					findValue.setValidators(Validators.required);
			});
	}
}
