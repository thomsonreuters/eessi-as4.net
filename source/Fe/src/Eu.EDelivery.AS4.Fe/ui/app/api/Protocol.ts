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

	static getForm(formBuilder: FormBuilder, current: Protocol): FormGroup {
		return formBuilder.group({
			[this.FIELD_url]: [current && current.url, Validators.required],
			[this.FIELD_useChunking]: [!!(current && current.useChunking), Validators.required],
			[this.FIELD_useHttpCompression]: [!!(current && current.useHttpCompression), Validators.required],
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: Protocol) {
		form.removeControl(this.FIELD_url);
		form.addControl(this.FIELD_url, formBuilder.control(current && current.url));
		form.removeControl(this.FIELD_useChunking);
		form.addControl(this.FIELD_useChunking, formBuilder.control(current && current.useChunking));
		form.removeControl(this.FIELD_useHttpCompression);
		form.addControl(this.FIELD_useHttpCompression, formBuilder.control(current && current.useHttpCompression));
	}
}
