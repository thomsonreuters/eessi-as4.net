/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Property } from "./Property";

export class ItemType {
	name: string;
	technicalName: string;
	properties: Property[];

	static FIELD_name: string = 'name';	
	static FIELD_technicalName: string = 'technicalName';	
	static FIELD_properties: string = 'properties';
}
