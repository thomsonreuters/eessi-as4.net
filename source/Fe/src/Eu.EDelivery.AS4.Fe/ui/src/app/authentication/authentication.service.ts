import { Http, Headers, RequestOptions } from '@angular/http';
import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import { JwtHelper, tokenNotExpired } from 'angular2-jwt';

const state = {
    loggedin: tokenNotExpired()
};

interface State {
    loggedin: boolean;
}
const store = new BehaviorSubject<State>(state);

@Injectable()
export class AuthenticationStore {
    public changes = store.asObservable();
    private store = store;
    public getState() {
        return this.store.value;
    }
    public setState(newState: State) {
        this.store.next(newState);
    }
}
const TOKENSTORE: string = 'id_token';

@Injectable()
export class AuthenticationService {
    constructor(private http: Http, private authenticationStore: AuthenticationStore, private jwtHelper: JwtHelper, private router: Router) {
    }
    public login(username: string, password: string): Observable<boolean> {
        let obs = new Subject<boolean>();
        let headers = new Headers({ 'Content-Type': 'application/json' });
        let options = new RequestOptions({ headers: headers });
        this.http.post('api/authentication', JSON.stringify({
            username: username,
            password: password
        }), options)
            .subscribe(result => {
                obs.next(true);
                localStorage.setItem(TOKENSTORE, result.json().access_token);
                this.router.navigate(['/settings']);
                this.authenticationStore.setState(Object.assign({}, {
                    loggedin: true
                }));

            }, () => {
                obs.next(false);
                localStorage.removeItem(TOKENSTORE);
                this.authenticationStore.setState(Object.assign({}, {
                    loggedin: false
                }));
            });
        return obs.asObservable();
    }
    public logout() {
        localStorage.removeItem(TOKENSTORE);
        this.authenticationStore.setState(Object.assign({}, {
            loggedin: false
        }));
        this.router.navigate(['/login']);
    }
}
