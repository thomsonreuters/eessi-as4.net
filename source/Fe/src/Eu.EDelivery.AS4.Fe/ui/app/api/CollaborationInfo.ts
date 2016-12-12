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
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: CollaborationInfo) {
		form.removeControl('action');
		form.addControl('action', formBuilder.control(current && current.action));
		form.removeControl('conversationId');
		form.addControl('conversationId', formBuilder.control(current && current.conversationId));

		form.removeControl('agreementRef');
		form.addControl('agreementRef', Agreement.getForm(formBuilder, current && current.agreementRef));
		form.removeControl('service');
		form.addControl('service', Service.getForm(formBuilder, current && current.service));
	}
}
