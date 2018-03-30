/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ItemType } from './ItemType';
import { Setting } from '.';

export class Transformer {
	type: string;
	setting: Setting[];

	static FIELD_type: string = 'type';
	static FIELD_setting: string = 'setting';

	constructor(init?: Partial<Transformer>) {
		Object.assign(this, init);
		this.setting = new Array<Setting>();
	}
}

export class TransformerConfigEntry {
	defaultTransformer: ItemType;
	otherTransformers: Array<ItemType>;
}
