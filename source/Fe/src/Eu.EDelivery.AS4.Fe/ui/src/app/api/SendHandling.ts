/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Method } from "./Method";

export class SendHandling {
	notifyMessageProducer: boolean;
	notifyMethod: Method;

	static FIELD_notifyMessageProducer: string = 'notifyMessageProducer';
	static FIELD_notifyMethod: string = 'notifyMethod';
}
