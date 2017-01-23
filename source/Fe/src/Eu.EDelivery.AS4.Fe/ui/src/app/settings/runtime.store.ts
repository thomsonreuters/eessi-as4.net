import { Store } from './../common/store';
import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { ItemType } from './../api/ItemType';

interface State {
    receivers: ItemType[];
    steps: ItemType[];
    transformers: ItemType[];
    certificateRepositories: ItemType[];
    deliverSenders: ItemType[];
    runtimeMetaData: ItemType[];
}

@Injectable()
export class RuntimeStore extends Store<State> {
    constructor() {
        super({
            receivers: Array<ItemType>(),
            steps: Array<ItemType>(),
            transformers: Array<ItemType>(),
            certificateRepositories: Array<ItemType>(),
            deliverSenders: Array<ItemType>(),
            runtimeMetaData: Array<ItemType>()
        });
    }
}
