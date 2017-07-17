/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Property {
	friendlyName: string;
	technicalName: string;
	type: string;
	regex: string;
	description: string;
	defaultValue: string;
	required: boolean;
	attributes: string[];

	static FIELD_friendlyName: string = 'friendlyName';
	static FIELD_type: string = 'type';
	static FIELD_regex: string = 'regex';
	static FIELD_description: string = 'description';
	static FIELD_technicalName = 'technicalName';
	static FIELD_defaultValue = 'defaultValue';
	static FIELD_required = 'required';
	static FIELD_attributes = 'attributes';
}
