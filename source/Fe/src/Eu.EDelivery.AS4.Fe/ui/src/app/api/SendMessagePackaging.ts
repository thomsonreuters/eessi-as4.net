/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { PartyInfo } from "./PartyInfo";
import { CollaborationInfo } from "./CollaborationInfo";
import { MessageProperty } from "./MessageProperty";

export class SendMessagePackaging {
	mpc: string;
	useAS4Compression: boolean;
	isMultiHop: boolean;
	includePModeId: boolean;
	partyInfo: PartyInfo;
	collaborationInfo: CollaborationInfo;
	messageProperties: MessageProperty[];

	static FIELD_mpc: string = 'mpc';	
	static FIELD_useAS4Compression: string = 'useAS4Compression';	
	static FIELD_isMultiHop: string = 'isMultiHop';	
	static FIELD_includePModeId: string = 'includePModeId';	
	static FIELD_partyInfo: string = 'partyInfo';
	static FIELD_collaborationInfo: string = 'collaborationInfo';
	static FIELD_messageProperties: string = 'messageProperties';

	static getForm(formBuilder: FormBuilder, current: SendMessagePackaging): FormGroup {
		return formBuilder.group({
			mpc: [current && current.mpc],
			useAS4Compression: [!!(current && current.useAS4Compression)],
			isMultiHop: [!!(current && current.isMultiHop)],
			includePModeId: [!!(current && current.includePModeId)],
			partyInfo: PartyInfo.getForm(formBuilder, current && current.partyInfo),
			collaborationInfo: CollaborationInfo.getForm(formBuilder, current && current.collaborationInfo),
			messageProperties: formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessageProperty.getForm(formBuilder, item))),
		});
	}
	/// Patch up all the formArray controls
	static patchForm(formBuilder: FormBuilder, form: FormGroup, current: SendMessagePackaging) {
		form.removeControl('mpc');
		form.addControl('mpc', formBuilder.control(current && current.mpc));
		form.removeControl('useAS4Compression');
		form.addControl('useAS4Compression', formBuilder.control(current && current.useAS4Compression));
		form.removeControl('isMultiHop');
		form.addControl('isMultiHop', formBuilder.control(current && current.isMultiHop));
		form.removeControl('includePModeId');
		form.addControl('includePModeId', formBuilder.control(current && current.includePModeId));

		form.removeControl('partyInfo');
		form.addControl('partyInfo', PartyInfo.getForm(formBuilder, current && current.partyInfo));
		form.removeControl('collaborationInfo');
		form.addControl('collaborationInfo', CollaborationInfo.getForm(formBuilder, current && current.collaborationInfo));
		form.removeControl('messageProperties');
		form.addControl('messageProperties', formBuilder.array(!!!(current && current.messageProperties) ? [] : current.messageProperties.map(item => MessageProperty.getForm(formBuilder, item))));
	}
}
