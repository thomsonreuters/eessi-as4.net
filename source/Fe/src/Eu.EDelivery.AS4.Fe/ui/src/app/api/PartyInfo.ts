/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { Party } from "./Party";

export class PartyInfo {
	fromParty: Party;
	toParty: Party;

	static FIELD_fromParty: string = 'fromParty';
	static FIELD_toParty: string = 'toParty';
}
