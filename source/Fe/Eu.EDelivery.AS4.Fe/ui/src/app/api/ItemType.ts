/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Property } from "./Property";

export class ItemType {
	name: string;
	technicalName: string;
	properties: Property[];
	defaultvalue: any;
	description: string;

	static FIELD_name: string = 'name';	
	static FIELD_technicalName: string = 'technicalName';	
	static FIELD_properties: string = 'properties';
	static FIELD_defaultValue: string = 'defaultvalue';
	static FIELD_description: string = 'description';
}
