/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Repository } from "./Repository";

export class CertificateStore {
	storeName: string;
	repository: Repository;

	static FIELD_storeName: string = 'storeName';	
	static FIELD_repository: string = 'repository';

	static getForm(formBuilder: FormBuilder, current: CertificateStore): FormGroup {
		return formBuilder.group({
			storeName: [current && current.storeName],
			repository: Repository.getForm(formBuilder, current && current.repository),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: CertificateStore) {
		form.removeControl('storeName');
		form.addControl('storeName', formBuilder.control(current && current.storeName));

		form.removeControl('repository');
		form.addControl('repository', Repository.getForm(formBuilder, current && current.repository));
	}
}
