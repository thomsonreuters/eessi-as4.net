import { SendingProcessingMode } from './../api/SendingProcessingMode';
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
import { IPmode } from './../api/Pmode.interface';
import { ReceivingProcessingMode } from './../api/ReceivingProcessingMode';

export interface ICrudPmodeService {
    obsGet(): Observable<IPmode>;
    obsGetAll(): Observable<Array<string>>;
    get(name: string);
    delete(name: string);
    getNew(name: string): IPmode;
    create(pmode: IPmode): Observable<boolean>;
    getForm(pmode: IPmode): FormGroup;
    getByName(name: string): Observable<IPmode>;
    patchForm(form: FormGroup, pmode: IPmode);
    patchName(form: FormGroup, name: string);
    update(pmode: IPmode, originalName: string): Observable<boolean>;
    getAll();
}

@Injectable()
export class ReceivingPmodeService implements ICrudPmodeService {
    private _baseUrl = 'api/pmode/receiving';
    constructor(private http: AuthHttp, private pmodeStore: PmodeStore, private formBuilder: FormBuilder) {

    }
    getAll() {
        this.http
            .get(this.getBaseUrl())
            .subscribe(result => {
                this.pmodeStore.update('ReceivingNames', result.json());
            });
    }
    obsGet(): Observable<IPmode> {
        return this.pmodeStore
            .changes
            .filter(result => !!result)
            .map(result => result.Receiving)
            .distinctUntilChanged();
    }
    obsGetAll(): Observable<Array<string>> {
        return this.pmodeStore
            .changes
            .filter(result => !!result)
            .map(result => result.ReceivingNames)
            .distinctUntilChanged();
    }
    get(name: string) {
        this.http
            .get(`${this.getBaseUrl(name)}`)
            .subscribe(result => this.pmodeStore.update('Receiving', result.json()));
    }
    delete(name: string) {
        this.http
            .delete(`${this.getBaseUrl(name)}/`)
            .subscribe(result => {
                this.pmodeStore.deleteSending(name);
            });
    }
    update(pmode: IPmode, originalName: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .put(`${this.getBaseUrl(originalName)}`, pmode)
            .subscribe(result => {
                this.pmodeStore.setReceiving(<ReceivingPmode>pmode);
                obs.next(true);
                obs.complete();
            });
        return obs.asObservable();
    }
    getNew(name: string): IPmode {
        let newPmode = new ReceivingPmode();
        newPmode.name = name;
        newPmode.pmode = new ReceivingProcessingMode();
        newPmode.pmode.id = name;
        newPmode.type = 0;
        return newPmode;
    }
    create(pmode: IPmode): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .post(`${this.getBaseUrl()}`, pmode)
            .subscribe(result => {
                this.pmodeStore.setReceiving(<ReceivingPmode>pmode);
                obs.next();
                obs.complete();
            });
        return obs.asObservable();
    }
    patchForm(form: FormGroup, pmode: IPmode) {
        ReceivingPmode.patchForm(this.formBuilder, form, <ReceivingPmode>pmode);
    }
    getForm(pmode: IPmode): FormGroup {
        return ReceivingPmode.getForm(this.formBuilder, <ReceivingPmode>pmode);
    }
    patchName(form: FormGroup, name: string) {
        form.setValue({ [ReceivingPmode.FIELD_name]: name });
    }
    getByName(name: string): Observable<IPmode> {
        let obs = new Subject<ReceivingPmode>();
        this.http
            .get(`${this.getBaseUrl(name)}`)
            .subscribe(result => {
                obs.next(result.json());
                obs.complete();
            });
        return obs.asObservable();
    }
    private getBaseUrl(action?: string): string {
        if (!!!action) return this._baseUrl;
        return `${this._baseUrl}/${action}`;
    }
}

@Injectable()
export class SendingPmodeService implements ICrudPmodeService {
    private _baseUrl = 'api/pmode/sending';
    constructor(private http: AuthHttp, private pmodeStore: PmodeStore, private formBuilder: FormBuilder) {

    }
    getAll() {
        this.http
            .get(this.getBaseUrl())
            .subscribe(result => {
                this.pmodeStore.update('SendingNames', result.json());
            });
    }
    obsGet(): Observable<IPmode> {
        return this.pmodeStore
            .changes
            .filter(result => !!result)
            .map(result => result.Sending)
            .distinctUntilChanged();
    }
    obsGetAll(): Observable<Array<string>> {
        return this.pmodeStore
            .changes
            .filter(result => !!result)
            .map(result => result.SendingNames)
            .distinctUntilChanged();
    }
    get(name: string) {
        this.http
            .get(`${this.getBaseUrl()}/${name}`)
            .subscribe(result => this.pmodeStore.update('Sending', result.json()));
    }
    delete(name: string) {
        this.http
            .delete(`${this.getBaseUrl()}/${name}`)
            .subscribe(result => {
                this.pmodeStore.deleteSending(name);
            });
    }
    update(pmode: IPmode, originalName: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .put(`${this.getBaseUrl()}/${originalName}`, pmode)
            .subscribe(result => {
                this.pmodeStore.setSending(<SendingPmode>pmode);
                obs.next(true);
                obs.complete();
            });
        return obs.asObservable();
    }
    getNew(name: string): IPmode {
        let newPmode = new SendingPmode();
        newPmode.name = name;
        newPmode.pmode = new SendingProcessingMode();
        newPmode.pmode.id = name;
        newPmode.type = 0;
        return newPmode;
    }
    create(pmode: IPmode): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .post(`${this.getBaseUrl()}`, pmode)
            .subscribe(result => {
                this.pmodeStore.setSending(<SendingPmode>pmode);
                obs.next();
                obs.complete();
            });
        return obs.asObservable();
    }
    patchForm(form: FormGroup, pmode: IPmode) {
        SendingPmode.patchForm(this.formBuilder, form, <SendingPmode>pmode);
    }
    getForm(pmode: IPmode): FormGroup {
        return SendingPmode.getForm(this.formBuilder, <SendingPmode>pmode);
    }
    patchName(form: FormGroup, name: string) {
        form.setValue({ [SendingPmode.FIELD_name]: name });
    }
    getByName(name: string): Observable<IPmode> {
        let obs = new Subject<SendingPmode>();
        this.http
            .get(`${this.getBaseUrl()}/${name}`)
            .subscribe(result => {
                obs.next(result.json());
                obs.complete();
            });
        return obs.asObservable();
    }
    private getBaseUrl(action?: string): string {
        if (!!!action) return this._baseUrl;
        return `${this._baseUrl}/action`;
    }
}
