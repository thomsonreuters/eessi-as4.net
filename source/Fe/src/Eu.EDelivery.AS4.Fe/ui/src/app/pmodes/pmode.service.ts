import { Http } from '@angular/http';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Observer } from 'rxjs/Observer';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Injectable, OpaqueToken } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { PmodeStore } from './pmode.store';
import { SendingPmode } from './../api/SendingPmode';
import { ReceivingPmode } from './../api/ReceivingPmode';

export interface IPmodeService {
    getAllReceiving();
    getAllSending();
    setSending(name: string);
    setReceiving(name: string);
    deleteReceiving(name: string);
    deleteSending(name: string);
    createReceiving(pmode: ReceivingPmode): Observable<boolean>;
    updateReceiving(pmode: ReceivingPmode, originalName: string): Observable<boolean>;
    createSending(pmode: SendingPmode): Observable<boolean>;
    updateSending(pmode: SendingPmode, originalName: string): Observable<boolean>;
    getSendingByName(name: string): Observable<SendingPmode>;
    getReceivingByName(name: string): Observable<ReceivingPmode>;
}

export let pmodeService = new OpaqueToken('pmodeService');

@Injectable()
export class PmodeService implements IPmodeService {
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
    public deleteReceiving(name: string) {
        this.http
            .delete(`${this.getBaseUrl('receiving')}/${name}`)
            .subscribe(result => {
                this.pmodeStore.deleteReceiving(name);
            });
    }
    public createReceiving(pmode: ReceivingPmode): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .post(`${this.getBaseUrl('receiving')}`, pmode)
            .subscribe(result => {
                this.pmodeStore.setReceiving(pmode);
                obs.next();
                obs.complete();
            });
        return obs.asObservable();
    }
    public updateReceiving(pmode: ReceivingPmode, originalName: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .put(`${this.getBaseUrl('receiving')}/${originalName}`, pmode)
            .subscribe(result => {
                this.pmodeStore.setReceiving(pmode);
                obs.next(true);
                obs.complete();
            });
        return obs.asObservable();
    }
    public deleteSending(name: string) {
        this.http
            .delete(`${this.getBaseUrl('sending')}/${name}`)
            .subscribe(result => {
                this.pmodeStore.deleteSending(name);
            });
    }
    public setSending(name: string) {
        this.http
            .get(`${this.getBaseUrl('sending')}/${name}`)
            .subscribe(result => this.pmodeStore.update('Sending', result.json()));
    }
    public setReceiving(name: string) {
        this.http
            .get(`${this.getBaseUrl('receiving')}/${name}`)
            .subscribe(result => this.pmodeStore.update('Receiving', result.json()));
    }
    public createSending(pmode: SendingPmode): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .post(`${this.getBaseUrl('sending')}`, pmode)
            .subscribe(result => {
                this.pmodeStore.setSending(pmode);
                obs.next();
                obs.complete();
            });
        return obs.asObservable();
    }
    public updateSending(pmode: SendingPmode, originalName: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .put(`${this.getBaseUrl('sending')}/${originalName}`, pmode)
            .subscribe(result => {
                this.pmodeStore.setSending(pmode);
                obs.next(true);
                obs.complete();
            });
        return obs.asObservable();
    }
    public getSendingByName(name: string): Observable<SendingPmode> {
        let obs = new Subject<SendingPmode>();
        this.http
            .get(`${this.getBaseUrl('sending')}/${name}`)
            .subscribe(result => obs.next(result.json()));
        return obs.asObservable();
    }
    public getReceivingByName(name: string): Observable<ReceivingPmode> {
        let obs = new Subject<ReceivingPmode>();
        this.http
            .get(`${this.getBaseUrl('receiving')}/${name}`)
            .subscribe(result => {
                obs.next(result.json());
                obs.complete();
            });
        return obs.asObservable();
    }
    private getBaseUrl(action: string): string {
        return `api/pmode/${action}`;
    }
}
