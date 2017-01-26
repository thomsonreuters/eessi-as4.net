import { PmodeStore } from './../pmodes/pmode.store';
import { RuntimeStore } from '../settings/runtime.store';
import { SettingsStore } from './../settings/settings.store';
import { DialogService } from './../common/dialog.service';
import { SpinnerService } from './../common/spinner/spinner.service';
import { Http, Headers, RequestOptions } from '@angular/http';
import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import { tokenNotExpired, JwtHelper } from 'angular2-jwt';

import { AuthenticationStore } from './authentication.store';
import { TOKENSTORE } from './token';

@Injectable()
export class AuthenticationService {
    constructor(private http: Http, private authenticationStore: AuthenticationStore, private router: Router, private _spinnerService: SpinnerService,
        private _dialogService: DialogService, private _settingsStore: SettingsStore, private _pmodesStore: PmodeStore, private _runtimeStore: RuntimeStore) {
    }
    public login(username: string, password: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers });
        this.http.post('api/authentication', JSON.stringify({
            username,
            password
        }), options)
            .subscribe((result) => {
                obs.next(true);
                let token = result.json().access_token;
                localStorage.setItem(TOKENSTORE, token);
                this.authenticationStore.login();
                this.router.navigate(['/settings']);
            }, (error: { status: number }) => {
                obs.next(false);
                localStorage.removeItem(TOKENSTORE);
                this.authenticationStore.logout();
                this._spinnerService.hide();

                if (error.status === 401) {
                    this._dialogService.error('Invalid username/password');
                }
            });
        return obs.asObservable();
    }
    public logout() {
        this._settingsStore.clear();
        this._pmodesStore.clear();
        this._runtimeStore.clear();

        localStorage.removeItem(TOKENSTORE);
        this.authenticationStore.logout();
        this.router.navigate(['/login']);
    }
}
