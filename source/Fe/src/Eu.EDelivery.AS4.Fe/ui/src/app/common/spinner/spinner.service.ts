import { AuthHttp } from 'angular2-jwt';
import { CustomHttp } from './../as4components.module';
import { Http, XHRBackend, RequestOptions } from '@angular/http';
import { Injectable, Provider } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

@Injectable()
export class SpinnerService {
    public changes: Observable<boolean>;
    private _changes = new BehaviorSubject<boolean>(false);
    private _counter: number = 0;
    constructor() {
        this.changes = this._changes.asObservable().distinctUntilChanged().last();
    }
    public show() {
        this._counter++;
        this.process();
    }
    public hide() {
        this._counter--;
        this.process();
    }
    private process() {
        this._changes.next(this._counter > 0);
    }
}

export const SPINNER_PROVIDERS: Provider[] = [
    {
        provide: Http, useFactory: (backend, requestOptions, spinnerService) => {
            return new CustomHttp(backend, requestOptions, spinnerService);
        }, deps: [XHRBackend, RequestOptions, SpinnerService]
    },
    {
        provide: AuthHttp, useFactory: (backend, requestOptions, spinnerService) => {
            return new CustomHttp(backend, requestOptions, spinnerService);
        }, deps: [XHRBackend, RequestOptions, SpinnerService]
    }
];
