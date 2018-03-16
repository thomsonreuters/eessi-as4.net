import { Http } from '@angular/http';
import { FormBuilder, FormGroup } from '@angular/forms';
import { Observer } from 'rxjs/Observer';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Subject } from 'rxjs/Subject';
import { Injectable, OpaqueToken } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { RuntimeStore } from './../settings/runtime.store';
import { PmodeStore } from './pmode.store';
import { SendingPmode } from './../api/SendingPmode';
import { ReceivingPmode } from './../api/ReceivingPmode';
import { ReceivingPmodeForm } from './../api/ReceivingPmodeForm';
import { IPmode } from './../api/Pmode.interface';
import { ReceivingProcessingMode } from './../api/ReceivingProcessingMode';
import { ICrudPmodeService } from './crudpmode.service.interface';
import { FormWrapper, FormBuilderExtended } from './../common/form.service';
import { SendingProcessingMode } from './../api/SendingProcessingMode';

@Injectable()
export class ReceivingPmodeService implements ICrudPmodeService {
    private _baseUrl = 'api/pmode/receiving';
    private _form: FormWrapper;
    constructor(private http: AuthHttp, private pmodeStore: PmodeStore, private formBuilder: FormBuilderExtended, private _runtimeStore: RuntimeStore) { }
    public getAll() {
        this.http
            .get(this.getBaseUrl())
            .subscribe((result) => {
                this.pmodeStore.update('ReceivingNames', result.json());
            });
    }
    public obsGet(): Observable<IPmode | undefined> {
        return this.pmodeStore
            .changes
            .filter((result) => !!result)
            .map((result) => result.Receiving)
            .distinctUntilChanged();
    }
    public obsGetAll(): Observable<string[] | undefined> {
        return this.pmodeStore
            .changes
            .filter((result) => !!result)
            .map((result) => result.ReceivingNames)
            .distinctUntilChanged();
    }
    public get(name: string) {
        if (!!!name) {
            this.pmodeStore.update('Receiving', null);
            return;
        }
        this.http
            .get(`${this.getBaseUrl(name)}`)
            .subscribe((result) => {
                this.pmodeStore.update('Receiving', result.json());
            });
    }
    public delete(name: string, onlyStore: boolean = false) {
        if (onlyStore) {
            this.pmodeStore.deleteReceiving(name);
            return;
        }
        this.http
            .delete(`${this.getBaseUrl(name)}/`)
            .subscribe((result) => {
                this.pmodeStore.deleteReceiving(name);
            });
    }
    public update(pmode: IPmode, originalName: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .put(`${this.getBaseUrl(originalName)}`, pmode)
            .subscribe((result) => {
                this.pmodeStore.setReceiving(<ReceivingPmode>pmode);
                obs.next(true);
                obs.complete();
            });
        return obs.asObservable();
    }
    public getNew(name: string): IPmode {
        let newPmode = new ReceivingPmode();
        newPmode.name = name;
        newPmode.pmode = new ReceivingProcessingMode();
        newPmode.pmode.id = name;
        newPmode.type = 0;
        return newPmode;
    }
    public create(pmode: IPmode): Observable<boolean> {
        let obs = new Subject<boolean>();
        this.http
            .post(`${this.getBaseUrl()}`, pmode)
            .subscribe((result) => {
                this.pmodeStore.setReceiving(<ReceivingPmode>pmode);
                obs.next();
                obs.complete();
            });
        return obs.asObservable();
    }
    public getForm(pmode: IPmode): FormWrapper {
        if (!!!this._form) {
            this._form = this.formBuilder.get();
        }
        return ReceivingPmodeForm.getForm(this._form, <ReceivingPmode>pmode, this._runtimeStore.state.runtimeMetaData);
    }
    public patchName(form: FormGroup, name: string) {
        form.patchValue({ [ReceivingPmode.FIELD_name]: name });
    }
    public getByName(name: string): Observable<IPmode> {
        let obs = new Subject<ReceivingPmode>();
        this.http
            .get(`${this.getBaseUrl(name)}`)
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
        return `${this._baseUrl}/${action}`;
    }
}
