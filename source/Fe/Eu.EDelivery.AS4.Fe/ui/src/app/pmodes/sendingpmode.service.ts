
import { Http } from '@angular/http';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Observer } from 'rxjs/Observer';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Injectable, OpaqueToken } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { FormBuilderExtended, FormWrapper } from './../common/form.service';
import { PmodeStore } from './pmode.store';
import { SendingPmode } from './../api/SendingPmode';
import { SendingPmodeForm } from './../api/SendingPmodeForm';
import { ReceivingPmode } from './../api/ReceivingPmode';
import { IPmode } from './../api/Pmode.interface';
import { ICrudPmodeService } from './crudpmode.service.interface';
import { RuntimeStore } from './../settings/runtime.store';
import { SendingProcessingMode } from './../api/SendingProcessingMode';

@Injectable()
export class SendingPmodeService implements ICrudPmodeService {
    private _baseUrl = 'api/pmode/sending';
    private _form: FormWrapper;
    constructor(private _http: AuthHttp, private _pmodeStore: PmodeStore, private _formBuilder: FormBuilderExtended, private _runtimeStore: RuntimeStore) { }
    public getAll() {
        this._http
            .get(this.getBaseUrl())
            .subscribe((result) => {
                this._pmodeStore.update('SendingNames', result.json());
            });
    }
    public obsGet(): Observable<IPmode | undefined> {
        return this
            ._pmodeStore
            .changes
            .filter((result) => !!result)
            .map((result) => result.Sending)
            .distinctUntilChanged();
    }
    public obsGetAll(): Observable<string[] | undefined> {
        return this._pmodeStore
            .changes
            .filter((result) => !!result)
            .map((result) => result.SendingNames)
            .distinctUntilChanged();
    }
    public get(name: string) {
        if (!!!name) {
            this._pmodeStore.update('Sending', null);
            return;
        }

        this._http
            .get(`${this.getBaseUrl()}/${name}`)
            .subscribe((result) => this._pmodeStore.update('Sending', result.json()));
    }
    public delete(name: string, onlyStore: boolean = false) {
        if (onlyStore) {
            this._pmodeStore.deleteSending(name);
            return;
        }
        this._http
            .delete(`${this.getBaseUrl()}/${name}`)
            .subscribe((result) => {
                this._pmodeStore.deleteSending(name);
            });
    }
    public update(pmode: IPmode, originalName: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        this._http
            .put(`${this.getBaseUrl()}/${originalName}`, pmode)
            .subscribe((result) => {
                this._pmodeStore.setSending(<SendingPmode>pmode);
                obs.next(true);
                obs.complete();
            });
        return obs.asObservable();
    }
    public getNew(name: string): IPmode {
        let newPmode = new SendingPmode();
        newPmode.name = name;
        newPmode.pmode = new SendingProcessingMode();
        newPmode.pmode.id = name;
        newPmode.type = 0;
        return newPmode;
    }
    public create(pmode: IPmode): Observable<boolean> {
        let obs = new Subject<boolean>();
        this._http
            .post(`${this.getBaseUrl()}`, pmode)
            .subscribe((result) => {
                this._pmodeStore.setSending(<SendingPmode>pmode);
                obs.next();
                obs.complete();
            });
        return obs.asObservable();
    }
    public getForm(pmode: IPmode): FormWrapper {
        if (!!!this._form) {
            this._form = this._formBuilder.get();
        }
        return SendingPmodeForm.getForm(this._form, <SendingPmode>pmode, this._runtimeStore.state.runtimeMetaData);
    }
    public patchName(form: FormGroup, name: string) {
        form.setValue({ [SendingPmode.FIELD_name]: name });
    }
    public getByName(name: string): Observable<IPmode> {
        let obs = new Subject<SendingPmode>();
        this._http
            .get(`${this.getBaseUrl()}/${name}`)
            .subscribe((result) => {
                obs.next(result.json());
                obs.complete();
            });
        return obs.asObservable();
    }
    private getBaseUrl(action?: string): string {
        if (!!!action) {
            return this._baseUrl;
        }
        return `${this._baseUrl}/action`;
    }
}
