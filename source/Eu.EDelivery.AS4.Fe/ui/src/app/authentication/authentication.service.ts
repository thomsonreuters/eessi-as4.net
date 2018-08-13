import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { Http, Headers, RequestOptions } from '@angular/http';
import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { tokenNotExpired, JwtHelper } from 'angular2-jwt';

import { LogoutService } from './logout.service';
import { DialogService } from './../common/dialog.service';
import { SpinnerService } from './../common/spinner/spinner.service';
import { AuthenticationStore } from './authentication.store';
import { TOKENSTORE } from './token';

@Injectable()
export class AuthenticationService {
    public onAuthenticate: Observable<boolean>;
    public get isAuthenticated(): boolean {
        return !!this.getToken();
    }
    private _onAuthenticate: BehaviorSubject<boolean> = new BehaviorSubject(false);
    constructor(private http: Http, private authenticationStore: AuthenticationStore, private router: Router, private _spinnerService: SpinnerService,
        private _dialogService: DialogService, private _logoutService: LogoutService) {
        this.onAuthenticate = this._onAuthenticate.asObservable();
        if (!!this.getToken()) {
            this._onAuthenticate.next(true);
        }
    }
    public getToken(): string | null {
        if (!tokenNotExpired(TOKENSTORE)) {
            return null;
        }
        return localStorage.getItem(TOKENSTORE);
    }
    public login(username: string, password: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers });
        this.http
            .post('api/authentication', JSON.stringify({
                username,
                password
            }), options)
            .subscribe((result) => {
                obs.next(true);
                let token = result.json().access_token;
                localStorage.setItem(TOKENSTORE, token);
                this.authenticationStore.login();
                this.router.navigate(['/settings']);
                this._onAuthenticate.next(true);
            }, (error: { status: number }) => {
                this._onAuthenticate.next(false);
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
        this._logoutService.logout();
    }
}
