import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { PmodeStore } from './pmode.store';

@Injectable()
export class PmodeService {
    constructor(private http: AuthHttp, private pmodeStore: PmodeStore) {

    }
    public getAllReceiving() {
        this.http
            .get(this.getBaseUrl('receiving'))
            .subscribe(result => {
                this.pmodeStore.update('ReceivingNames', result.json());
            });
    }
    public getAllSending() {
        this.http
            .get(this.getBaseUrl('sending'))
            .subscribe(result => {
                this.pmodeStore.update('SendingNames', result.json());
            });
    }
    public getSending(name: string) {
        this.http
            .get(`${this.getBaseUrl('sending')}/${name}`)
            .subscribe(result => this.pmodeStore.update('Sending', result.json()));
    }
    public getReceiving(name: string) {
        this.http
            .get(`${this.getBaseUrl('receiving')}/${name}`)
            .subscribe(result => this.pmodeStore.update('Receiving', result.json()));
    }
    private getBaseUrl(action: string): string {
        return `api/pmode/${action}`;
    }
}
