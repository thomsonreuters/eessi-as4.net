/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';

export class SettingsDatabase {
	provider: string;
	connectionString: string;

	static FIELD_provider: string = 'provider';	
	static FIELD_connectionString: string = 'connectionString';		
}
