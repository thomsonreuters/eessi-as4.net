import { Store } from './../common/store';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { ItemType } from './runtime.service';

interface State {
    receivers: Array<ItemType>,
    steps: Array<ItemType>,
    transformers: Array<ItemType>
}

@Injectable()
export class RuntimeStore extends Store<State> {
    constructor() {
        super({
            receivers: Array<ItemType>(),
            steps: Array<ItemType>(),
            transformers: Array<ItemType>()
        });
    }
}
