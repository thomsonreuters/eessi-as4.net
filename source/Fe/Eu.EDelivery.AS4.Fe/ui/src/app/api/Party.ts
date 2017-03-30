/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { PartyId } from "./PartyId";

export class Party {
	role: string;
	partyIds: PartyId[];

	static FIELD_role: string = 'role';
	static FIELD_partyIds: string = 'partyIds';
}
