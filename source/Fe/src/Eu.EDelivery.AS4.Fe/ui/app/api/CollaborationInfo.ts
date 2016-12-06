/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Agreement } from "./Agreement";
import { Service } from "./Service";

export class CollaborationInfo {
	action: string;
	conversationId: string;
	agreementRef: Agreement;
	service: Service;

	static FIELD_action: string = 'action';	
	static FIELD_conversationId: string = 'conversationId';	
	static FIELD_agreementRef: string = 'agreementRef';
	static FIELD_service: string = 'service';

	static getForm(formBuilder: FormBuilder, current: CollaborationInfo): FormGroup {
		return formBuilder.group({
			action: [current && current.action],
			conversationId: [current && current.conversationId],
			agreementRef: Agreement.getForm(formBuilder, current && current.agreementRef),
			service: Service.getForm(formBuilder, current && current.service),
		});
	}
	/// Patch up all the formArray controls
	static patchFormArrays(formBuilder: FormBuilder, form: FormGroup, current: CollaborationInfo) {
		Agreement.patchFormArrays(formBuilder, <FormGroup>form.controls['agreementRef'], current && current.agreementRef);
		Service.patchFormArrays(formBuilder, <FormGroup>form.controls['service'], current && current.service);
	}
}
