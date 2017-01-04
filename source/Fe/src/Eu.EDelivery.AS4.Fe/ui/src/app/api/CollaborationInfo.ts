/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Agreement } from "./Agreement";
import { Service } from "./Service";

export class CollaborationInfo {
	action: string;
	conversationId: string;
	agreementReference: Agreement;
	service: Service;

	static FIELD_action: string = 'action';
	static FIELD_conversationId: string = 'conversationId';
	static FIELD_agreementReference: string = 'agreementReference';
	static FIELD_service: string = 'service';

	static getForm(formBuilder: FormBuilder, current: CollaborationInfo): FormGroup {
		return formBuilder.group({
			[this.FIELD_action]: [current && current.action],
			[this.FIELD_conversationId]: [current && current.conversationId],
			[this.FIELD_agreementReference]: Agreement.getForm(formBuilder, current && current.agreementReference),
			[this.FIELD_service]: Service.getForm(formBuilder, current && current.service),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: CollaborationInfo) {
		form.removeControl(this.FIELD_action);
		form.addControl(this.FIELD_action, formBuilder.control(current && current.action));
		form.removeControl(this.FIELD_conversationId);
		form.addControl(this.FIELD_conversationId, formBuilder.control(current && current.conversationId));

		form.removeControl(this.FIELD_agreementReference);
		form.addControl(this.FIELD_agreementReference, Agreement.getForm(formBuilder, current && current.agreementReference));
		form.removeControl(this.FIELD_service);
		form.addControl(this.FIELD_service, Service.getForm(formBuilder, current && current.service));
	}
}
