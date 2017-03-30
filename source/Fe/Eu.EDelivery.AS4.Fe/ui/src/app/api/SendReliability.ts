/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ReceptionAwareness } from "./ReceptionAwareness";

export class SendReliability {
	receptionAwareness: ReceptionAwareness = new ReceptionAwareness();

	static FIELD_receptionAwareness: string = 'receptionAwareness';
}
