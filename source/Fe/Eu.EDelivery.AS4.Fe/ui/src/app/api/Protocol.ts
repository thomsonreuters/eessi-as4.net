/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Validators } from '@angular/forms';

export class Protocol {
	url: string;
	useChunking: boolean;
	useHttpCompression: boolean;

	static FIELD_url: string = 'url';
	static FIELD_useChunking: string = 'useChunking';
	static FIELD_useHttpCompression: string = 'useHttpCompression';	
}
