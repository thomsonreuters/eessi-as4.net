import { Injectable } from '@angular/core';

import { Store } from './../common/store';
import { SendingPmode } from './../api/SendingPmode';
import { ReceivingPmode } from './../api/ReceivingPmode';

export interface IPmodeStore {
    Receiving: ReceivingPmode | undefined;
    Sending: SendingPmode | undefined;
    ReceivingNames: string[] | undefined;
    SendingNames: string[] | undefined;
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
    public clear() {
        this.setState({
            Receiving: null,
            Sending: null,
            ReceivingNames: new Array<string>(),
            SendingNames: new Array<string>()
        });
    }
    public setReceiving(pmode: ReceivingPmode | undefined) {
        this.update('Receiving', pmode);
    }
    public setSending(pmode: SendingPmode | undefined) {
        this.update('Sending', pmode);
    }
    public setReceivingNames(names: string[] | undefined) {
        this.update('ReceivingNames', names);
    }
    public deleteReceiving(name: string) {
        this.setState({
            Receiving: null,
            Sending: this.state.Sending,
            ReceivingNames: this.state.ReceivingNames.filter(map => map !== name),
            SendingNames: this.state.SendingNames
        });
    }
    public deleteSending(name: string) {
        this.setState({
            Receiving: this.state.Receiving,
            ReceivingNames: this.state.ReceivingNames,
            Sending: null,
            SendingNames: this.state.SendingNames.filter(map => map !== name)
        });
    }
}
