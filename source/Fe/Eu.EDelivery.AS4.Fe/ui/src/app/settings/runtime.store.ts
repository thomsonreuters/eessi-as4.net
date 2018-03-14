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
    notifySenders: ItemType[],
    attachmentUploaders: ItemType[];
    dynamicDiscoveryProfiles: ItemType[];
    runtimeMetaData: ItemType[];
}

@Injectable()
export class RuntimeStore extends Store<State> {
    constructor() {
        super({
            receivers: new Array<ItemType>(),
            steps: new Array<ItemType>(),
            transformers: new Array<ItemType>(),
            certificateRepositories: new Array<ItemType>(),
            deliverSenders: new Array<ItemType>(),
            notifySenders: new Array<ItemType>(),
            attachmentUploaders: new Array<ItemType>(),
            dynamicDiscoveryProfiles: new Array<ItemType>(),
            runtimeMetaData: new Array<ItemType>()
        });
    }
    public clear() {
        this.setState({
            receivers: new Array<ItemType>(),
            steps: new Array<ItemType>(),
            transformers: new Array<ItemType>(),
            certificateRepositories: new Array<ItemType>(),
            deliverSenders: new Array<ItemType>(),
            notifySenders: new Array<ItemType>(),
            attachmentUploaders: new Array<ItemType>(),
            dynamicDiscoveryProfiles: new Array<ItemType>(),
            runtimeMetaData: new Array<ItemType>()
        });
    }
}
