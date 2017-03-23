/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Method } from "./Method";

export class Deliver {
	isEnabled: boolean;
	payloadReferenceMethod: Method;
	deliverMethod: Method;

	static FIELD_isEnabled: string = 'isEnabled';
	static FIELD_payloadReferenceMethod: string = 'payloadReferenceMethod';
	static FIELD_deliverMethod: string = 'deliverMethod';	
}
