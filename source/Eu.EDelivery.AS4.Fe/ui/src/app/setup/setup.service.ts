import { Observable } from 'rxjs/Observable';
import { Http } from '@angular/http';
import { Injectable } from '@angular/core';

import { SpinnerService } from './../common/spinner/spinner.service';
import { Settings } from './../api/Settings';

@Injectable()
export class SetupService {
    constructor(private _http: Http, private _spinnerService: SpinnerService) { }
    public isSetup(): Observable<boolean> {
        return this._http
            .get(this.getBaseUrl())
            .map((result) => result.json());
    }
    public save(setting: Settings): Observable<boolean> {
        this._spinnerService.show();
        return this
            ._http
            .post(this.getBaseUrl(), setting)
            .finally(() => this._spinnerService.hide())
            .map(() => true);
    }
    private getBaseUrl() {
        return `api/configuration/setup`;
    }
}
