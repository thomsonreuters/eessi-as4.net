/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { PartyInfo } from "./PartyInfo";
import { CollaborationInfo } from "./CollaborationInfo";
import { MessageProperty } from "./MessageProperty";

export class MessagePackaging {
	partyInfo: PartyInfo;
	collaborationInfo: CollaborationInfo;
	messageProperties: MessageProperty[];

	static FIELD_partyInfo: string = 'partyInfo';
	static FIELD_collaborationInfo: string = 'collaborationInfo';
	static FIELD_messageProperties: string = 'messageProperties';

	static getForm(formBuilder: FormBuilder, current: MessagePackaging): FormGroup {
		return formBuilder.group({
			partyInfo: PartyInfo.getForm(formBuilder, current && current.partyInfo),
			collaborationInfo: CollaborationInfo.getForm(formBuilder, current && current.collaborationInfo),
			messageProperties: formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessageProperty.getForm(formBuilder, item))),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: MessagePackaging) {
		form.get(this.FIELD_partyInfo).reset({ value: current && current.partyInfo, disabled: !!!current && form.parent.disabled });
		PartyInfo.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_partyInfo), current && current.partyInfo);
		CollaborationInfo.patchForm(formBuilder, <FormGroup>form.get(this.FIELD_collaborationInfo), current && current.collaborationInfo);
		form.setControl('messageProperties', formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessageProperty.getForm(formBuilder, item))));
	}
}
