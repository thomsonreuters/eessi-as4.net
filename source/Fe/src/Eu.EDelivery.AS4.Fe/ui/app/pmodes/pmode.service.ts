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
    getSending(name: string);
    getReceiving(name: string);
    deleteReceiving(name: string);
    deleteSending(name: string);
    createReceiving(pmode: ReceivingPmode): Observable<boolean>;
    updateReceiving(pmode: ReceivingPmode, originalName: string): Observable<boolean>;
    createSending(pmode: SendingPmode): Observable<boolean>;
    updateSending(pmode: SendingPmode, originalName: string): Observable<boolean>;
}

export let pmodeService = new OpaqueToken('pmodeService');

export interface ITestPmodeService {
    currentPmode: Observable<ReceivingPmode | SendingPmode>;
    getAll();
    get(name: string);
    delete(name: string);
    patchFormArrays(formBuilder: FormBuilder, formGroup: FormGroup, current: ReceivingPmode | SendingPmode);
}

@Injectable()
export class SendingPmodeService implements ITestPmodeService {
    public currentPmode: Observable<SendingPmode>;
    getAll() {

    }
    get(name: string) {

    }
    delete(name: string) {

    }
    public patchFormArrays(formBuilder: FormBuilder, formGroup: FormGroup, current: SendingPmode) {
        // SendingPmode.patchFormArrays(formBuilder, formGroup, current);
    }
    private getBaseUrl(action: string): string {
        return `api/pmode/sending/${action}`;
    }
}

@Injectable()
export class ReceivingPmodeService implements ITestPmodeService {
    public currentPmode: Observable<ReceivingPmode>;
    constructor(private http: AuthHttp, private pmodeStore: PmodeStore) {
        this.currentPmode = pmodeStore.changes.filter(result => !!result).map(result => result.Receiving);
    }
    getAll() {
        this.http
            .get(this.getBaseUrl())
            .subscribe(result => {
                this.pmodeStore.update('ReceivingNames', result.json());
            });
    }
    get(name: string) {
        this.http
            .get(`${this.getBaseUrl()}/${name}`)
            .subscribe(result => this.pmodeStore.update('Sending', result.json()));
    }
    delete(name: string) {

    }
    public patchFormArrays(formBuilder: FormBuilder, formGroup: FormGroup, current: ReceivingPmode) {
        ReceivingPmode.patchForm(formBuilder, formGroup, current);
    }
    private getBaseUrl(): string {
        return `api/pmode/receiving`;
    }
}

@Injectable()
export class PmodeService implements IPmodeService {
    constructor(private http: AuthHttp, private pmodeStore: PmodeStore, public receivingPmodeService: ReceivingPmodeService, public sendingPmodeservice: SendingPmodeService) {

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
    private getBaseUrl(action: string): string {
        return `api/pmode/${action}`;
    }
}
