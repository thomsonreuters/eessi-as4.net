/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

export class BaseSettings {
	idFormat: string;
	certificateStoreName: string;

	static getForm(formBuilder: FormBuilder, current: BaseSettings): FormGroup {
		return formBuilder.group({
				idFormat: [current && current.idFormat],
				certificateStoreName: [current && current.certificateStoreName],
		});
	}
}
