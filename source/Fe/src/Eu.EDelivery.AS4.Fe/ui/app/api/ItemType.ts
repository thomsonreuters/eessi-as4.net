/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Property } from "./Property"

export class ItemType {
	name: string;
	properties: Property[];
	technicalName: string;

	static FIELD_name: string = 'name';
	static FIELD_properties: string = 'properties';
	static FIELD_technicalName: string = 'technicalName';

	static getForm(formBuilder: FormBuilder, current: ItemType): FormGroup {
		return formBuilder.group({
			name: [current && current.name],
			properties: formBuilder.array(!!!(current && current.properties) ? [] : current.properties.map(item => Property.getForm(formBuilder, item))),
			technicalName: [current && current.technicalName]
		});
	}
}
