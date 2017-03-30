/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Agreement } from "./Agreement";
import { Service } from "./Service";
import { AgreementForm } from './AgreementForm';

export class CollaborationInfo {
	action: string;
	conversationId: string;
	agreementReference: Agreement;
	service: Service;

	static FIELD_action: string = 'action';
	static FIELD_conversationId: string = 'conversationId';
	static FIELD_agreementReference: string = 'agreementReference';
	static FIELD_service: string = 'service';
}
