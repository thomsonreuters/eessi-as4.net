import { Injectable } from '@angular/core';

import { Store } from './../common/store';
import { SendingPmode } from './../api/SendingPmode';
import { ReceivingPmode } from './../api/ReceivingPmode';

export interface IPmodeStore {
    Receiving: ReceivingPmode | null;
    Sending: SendingPmode | null;
    ReceivingNames: string[];
    SendingNames: string[];
}

@Injectable()
export class PmodeStore extends Store<IPmodeStore> {
    constructor() {
        super({
            Receiving: null,
            Sending: null,
            ReceivingNames: new Array<string>(),
            SendingNames: new Array<string>()
        });
    }
}
