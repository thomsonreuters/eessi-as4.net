import { Http, Headers, RequestOptions } from '@angular/http';
import { Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { Observable, BehaviorSubject, Subject } from 'rxjs';
import { JwtHelper, tokenNotExpired } from 'angular2-jwt';

import { AuthenticationStore, TOKENSTORE } from './authentication.store';

@Injectable()
export class AuthenticationService {
    constructor(private http: Http, private authenticationStore: AuthenticationStore, private jwtHelper: JwtHelper, private router: Router) {
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
