import { Http, Headers, RequestOptions } from '@angular/http';
import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import {
    JwtHelper
} from 'angular2-jwt';

const state = {
    loggedin: false
}
interface State {
    loggedin: boolean
}
const store = new BehaviorSubject<any>(state);

@Injectable()
export class AuthenticationStore {
    private store = store;
    public changes = store.asObservable();
    public getState() {
        return this.store.value;
    }
    public setState(state: State) {
        this.store.next(state);
    }
}
const TOKENSTORE: string = 'id_token';

@Injectable()
export class AuthenticationService {
    constructor(private http: Http, private authenticationStore: AuthenticationStore, private jwtHelper: JwtHelper, private router: Router) {
    }
    public login(username: string, password: string): Observable<boolean> {
        var obs = new Subject<boolean>();
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });
        this.http.post('api/authentication', JSON.stringify({
            username: username,
            password: password
        }), options)
            .subscribe(result => {
                obs.next(true);
                var state = {
                    loggedin: false
                }
                localStorage.setItem(TOKENSTORE, result.json().access_token);
                this.router.navigate(['/settings']);
                this.authenticationStore.setState(Object.assign({}, state))

            }, () => {
                obs.next(false);
                localStorage.removeItem(TOKENSTORE);
                this.authenticationStore.setState(Object.assign({}, state));
            });
        return obs.asObservable();
    }
    public logout() {
        localStorage.removeItem(TOKENSTORE);
        var state = {
            loggedin: false
        }
        this.authenticationStore.setState(Object.assign({}, state));
        this.router.navigate(['/login']);
    }
}