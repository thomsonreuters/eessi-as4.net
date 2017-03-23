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
}
