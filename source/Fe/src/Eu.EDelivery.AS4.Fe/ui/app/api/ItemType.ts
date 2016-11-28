/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Property } from "./Property"

export class ItemType {
		name: string;

		properties: Property[];

	static getForm(formBuilder: FormBuilder, current: ItemType): FormGroup {
		return formBuilder.group({
			name: [''],
			properties: formBuilder.array(!!!(current && current.properties) ? [] : current.properties.map(item => Property.getForm(formBuilder, item))),
		});
	}
}
