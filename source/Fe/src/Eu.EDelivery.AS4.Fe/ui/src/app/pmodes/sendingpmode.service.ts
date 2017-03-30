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
import { SendingPmodeForm } from './../api/SendingPmodeForm';
import { ReceivingPmode } from './../api/ReceivingPmode';
import { IPmode } from './../api/Pmode.interface';
import { ICrudPmodeService } from './crudpmode.service.interface';

@Injectable()
export class SendingPmodeService implements ICrudPmodeService {
    private _baseUrl = 'api/pmode/sending';
    constructor(private http: AuthHttp, private pmodeStore: PmodeStore, private formBuilder: FormBuilder) {

    }
    public getAll() {
        this.http
            .get(this.getBaseUrl())
            .subscribe((result) => {
                this.pmodeStore.update('SendingNames', result.json());
            });
    }
    public obsGet(): Observable<IPmode> {
        return this
            .pmodeStore
            .changes
            .filter((result) => !!result)
            .map((result) => result.Sending)
            .distinctUntilChanged();
    }
    public obsGetAll(): Observable<string[]> {
        return this.pmodeStore
            .changes
            .filter((result) => !!result)
            .map((result) => result.SendingNames)
            .distinctUntilChanged();
    }
    public get(name: string) {
        if (!!!name) {
            this.pmodeStore.update('Sending', null);
            return;
        }

        this.http
            .get(`${this.getBaseUrl()}/${name}`)
            .subscribe((result) => this.pmodeStore.update('Sending', result.json()));
    }
    public delete(name: string) {
        this.http
            .delete(`${this.getBaseUrl()}/${name}`)
            .subscribe((result) => {
                this.pmodeStore.deleteSending(name);
            });
    }
    public update(pmode: IPmode, originalName: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .put(`${this.getBaseUrl()}/${originalName}`, pmode)
            .subscribe((result) => {
                this.pmodeStore.setSending(<SendingPmode>pmode);
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
        this.http
            .post(`${this.getBaseUrl()}`, pmode)
            .subscribe((result) => {
                this.pmodeStore.setSending(<SendingPmode>pmode);
                obs.next();
                obs.complete();
            });
        return obs.asObservable();
    }
    public patchForm(form: FormGroup, pmode: IPmode) {
        SendingPmodeForm.patchForm(this.formBuilder, form, <SendingPmode>pmode);
    }
    public getForm(pmode: IPmode): FormGroup {
        return SendingPmodeForm.getForm(this.formBuilder, <SendingPmode>pmode);
    }
    public patchName(form: FormGroup, name: string) {
        form.setValue({ [SendingPmode.FIELD_name]: name });
    }
    public getByName(name: string): Observable<IPmode> {
        let obs = new Subject<SendingPmode>();
        this.http
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
