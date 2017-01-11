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
		form.get(this.FIELD_action).reset({ value: current && current.action, disabled: !!!current && form.parent.disabled });
		form.get(this.FIELD_conversationId).reset({ value: current && current.conversationId, disabled: !!!current && form.parent.disabled });
		Agreement.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_agreementReference), current && current.agreementReference);
		Service.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_service), current && current.service);
	}
}
