import { Http } from '@angular/http';
import { Observer } from 'rxjs/Observer';
import { Subject } from 'rxjs/Subject';
import { ItemType } from './../api/ItemType';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/toPromise';

import { RuntimeStore } from './runtime.store';

export interface IRuntimeService {
    getReceivers();
    getSteps();
    getTransformers();
    getCertificateRepositories();
    getAll();
}

@Injectable()
export class RuntimeService implements IRuntimeService {
    public isLoaded: boolean = false;
    private _runtimeMetaData: any | null = null;
    constructor(private _authHttp: AuthHttp, private _http: Http, private _runtimeStore: RuntimeStore) { }
    public getReceivers() {
        this._authHttp
            .get(this.getBaseUrl('getreceivers'))
            .subscribe((type) => this._runtimeStore.update('receivers', type.json()));
    }
    public getSteps() {
        this._authHttp
            .get(this.getBaseUrl('getsteps'))
            .subscribe((type) => this._runtimeStore.update('steps', type.json()));
    }
    public getTransformers() {
        this._authHttp
            .get(this.getBaseUrl('gettransformers'))
            .subscribe((type) => this._runtimeStore.update('transformers', type.json()));
    }
    public getCertificateRepositories() {
        this._authHttp
            .get(this.getBaseUrl('getcertificaterepositories'))
            .subscribe((type) => this._runtimeStore.update('certificateRepositories', type.json()));
    }
    public getDynamicDiscoveryProfiles() {
        this._authHttp
            .get(this.getBaseUrl('getdynamicdiscoveryprofiles'))
            .subscribe((type) => this._runtimeStore.update('dynamicdiscoveryprofiles', type.json()));
    }
    public getDescriptionByTechnicalName(technicalName: string): ItemType | undefined {
        // Flatten all types
        let result = Object
            .keys(this._runtimeStore.state)
            .map((key) => (<ItemType[]>this._runtimeStore.state[key]))
            .reduce((a, b) => a.concat(b))
            .find((item) => item.technicalName === technicalName);
        return result;
    }
    public getAll(): Promise<boolean> {
        return new Promise((resolve, reject) => {
            this._http
                .get(this.getBaseUrl('getall'))
                .subscribe((result) => {
                    let json = result.json();
                    this._runtimeStore.setState({
                        receivers: json.receivers,
                        steps: json.steps,
                        transformers: json.transformers,
                        certificateRepositories: json.certificateRepositories,
                        deliverSenders: json.deliverSenders,
                        notifySenders: json.notifySenders,
                        attachmentUploaders: json.attachmentUploaders,
                        dynamicDiscoveryProfiles: json.dynamicDiscoveryProfiles,
                        runtimeMetaData: json.runtimeMetaData
                    });
                    this.isLoaded = true;
                    resolve(true);
                }, () => {
                    resolve(true);
                });
        });
    }
    public getRuntimeMetaData(): Observable<any> {
        if (!!this._runtimeMetaData) {
            return Observable.of<any>(this._runtimeMetaData);
        }
        let obs = new Subject<any>();
        this._authHttp
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
