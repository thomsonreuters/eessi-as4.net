/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';


export class BaseSettings {
	idFormat: string;
	certificateStoreName: string;

	static FIELD_idFormat: string = 'idFormat';
	static FIELD_certificateStoreName: string = 'certificateStoreName';

	static getForm(formBuilder: FormBuilder, current: BaseSettings): FormGroup {
		return formBuilder.group({
			idFormat: [current && current.idFormat],
			certificateStoreName: [current && current.certificateStoreName],
		});
	}
}
