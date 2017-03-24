/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { CertificateStore } from "./CertificateStore";

export class BaseSettings {
	idFormat: string;
	certificateStore: CertificateStore;

	static FIELD_idFormat: string = 'idFormat';
	static FIELD_certificateStore: string = 'certificateStore';
}
