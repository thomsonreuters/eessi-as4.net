import { AuthHttp } from 'angular2-jwt';
import { Http, XHRBackend, RequestOptions } from '@angular/http';
import { Injectable, Provider } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { DialogService } from './../dialog.service';
import { CustomHttp } from './customhttp';

@Injectable()
export class SpinnerService {
    public changes: Observable<boolean>;
    private _changes = new BehaviorSubject<boolean>(false);
    private _counter: number = 0;
    constructor() {
        this.changes = this._changes.asObservable().distinctUntilChanged();
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

// tslint:disable-next-line:max-line-length
export function spinnerHttpServiceFactory(backend: XHRBackend, options: RequestOptions, spinnerService: SpinnerService, dialogService: DialogService) {
    return new CustomHttp(backend, options, spinnerService, dialogService);
}
