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
}
