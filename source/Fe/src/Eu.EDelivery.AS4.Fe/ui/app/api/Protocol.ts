/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class Protocol {
	url: string;
	useChunking: boolean;
	useHttpCompression: boolean;

	static FIELD_url: string = 'url';	
	static FIELD_useChunking: string = 'useChunking';	
	static FIELD_useHttpCompression: string = 'useHttpCompression';	

	static getForm(formBuilder: FormBuilder, current: Protocol): FormGroup {
		return formBuilder.group({
			url: [current && current.url],
			useChunking: [!!(current && current.useChunking)],
			useHttpCompression: [!!(current && current.useHttpCompression)],
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: Protocol) {
	}
}
