/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Repository } from "./Repository";

export class CertificateStore {
	storeName: string;
	repository: Repository;

	static FIELD_storeName: string = 'storeName';
	static FIELD_repository: string = 'repository';
}
