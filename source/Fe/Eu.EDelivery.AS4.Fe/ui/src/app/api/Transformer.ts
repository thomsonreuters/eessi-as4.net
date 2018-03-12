/* tslint:disable */
import { FormBuilder, FormGroup, FormArray } from '@angular/forms';
import { ItemType } from './ItemType';

export class Transformer {
	type: string;

	static FIELD_type: string = 'type';

	constructor(init?: Partial<Transformer>) {
		Object.assign(this, init);
	}
}

export class TransformerConfigEntry {
	defaultTransformer: ItemType;
	otherTransformers: Array<ItemType>;
}
