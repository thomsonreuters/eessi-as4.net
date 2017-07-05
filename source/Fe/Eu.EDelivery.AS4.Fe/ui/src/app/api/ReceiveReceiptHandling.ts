import { FormControl, ControlValueAccessor } from '@angular/forms';
/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Validators } from '@angular/forms';

export class ReceiveReceiptHandling {
	useNNRFormat: boolean;
	replyPattern: number;
	sendingPMode: string;

	static FIELD_useNNRFormat: string = 'useNNRFormat';
	static FIELD_replyPattern: string = 'replyPattern';
	static FIELD_sendingPMode: string = 'sendingPMode';	
}
