import { Observer } from 'rxjs/Observer';
import { Subject } from 'rxjs/Subject';
import { ItemType } from './../api/ItemType';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { RuntimeStore } from './runtime.store';

export interface IRuntimeService {
    getReceivers();
    getSteps();
    getTransformers();
    getCertificateRepositories();
    getAll();
}

@Injectable()
export class RuntimeService implements IRuntimeService {s
    private _runtimeMetaData: any | null = null;
    constructor(private http: AuthHttp, private runtimeStore: RuntimeStore) {

    }
    public getReceivers() {
        this.http
            .get(this.getBaseUrl('getreceivers'))
            .subscribe((type) => this.runtimeStore.update('receivers', type.json()));
    }
    public getSteps() {
        this.http
            .get(this.getBaseUrl('getsteps'))
            .subscribe((type) => this.runtimeStore.update('steps', type.json()));
    }
    public getTransformers() {
        this.http
            .get(this.getBaseUrl('gettransformers'))
            .subscribe((type) => this.runtimeStore.update('transformers', type.json()));
    }
    public getCertificateRepositories() {
        this.http
            .get(this.getBaseUrl('getcertificaterepositories'))
            .subscribe((type) => this.runtimeStore.update('certificateRepositories', type.json()));
    }
    public getAll() {
        this.http
            .get(this.getBaseUrl('getall'))
            .subscribe((result) => {
                let json = result.json();
                this.runtimeStore.setState({
                    receivers: json.receivers,
                    steps: json.steps,
                    transformers: json.transformers,
                    certificateRepositories: json.certificateRepositories,
                    deliverSenders: json.deliverSenders,
                    runtimeMetaData: json.runtimeMetaData
                });
            });
    }
    public getRuntimeMetaData(): Observable<any> {
        if (!!this._runtimeMetaData) {
            return Observable.of<any>(this._runtimeMetaData);
        }
        let obs = new Subject<any>();
        this.http
            .get(this.getBaseUrl('getruntimemetadata'))
            .subscribe((result) => {
                this._runtimeMetaData = result.json();
                obs.next(this._runtimeMetaData);
                obs.complete();
            });
        return obs;
    }
    private getBaseUrl(action: string) {
        return `api/runtime/${action}`;
    }
}
