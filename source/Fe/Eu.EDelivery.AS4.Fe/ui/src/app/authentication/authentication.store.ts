import { Injectable, Inject } from '@angular/core';
import { JwtHelper, tokenNotExpired } from 'angular2-jwt';
import { Observable, BehaviorSubject } from 'rxjs';

import { Store } from './../common/store';
import { TOKENSTORE } from './token';

const state = {
    loggedin: tokenNotExpired(),
    roles: new Array<string>()
};

export interface State {
    loggedin: boolean;
    roles: string[];
}

@Injectable()
export class AuthenticationStore extends Store<State> {
    constructor(private _jwtHelper: JwtHelper) {
        super(state);
        if (tokenNotExpired()) {
            this.login();
        }
    }
    public login() {
        let tokenFromStore = localStorage.getItem(TOKENSTORE);
        if (!!!tokenFromStore) {
            throw new Error('No token found in store!');
        }
        let token = this._jwtHelper.decodeToken(tokenFromStore);
        this.setState({
            loggedin: true,
            roles: this.getRoles(token)
        });
    }
    public logout() {
        this.update('loggedin', false);
    }
    public setRoles(roles: string[]) {
        this.update('roles', roles);
    }
    private getRoles(token: string): string[] {
        let result = new Array<string>();
        Object.keys(token).forEach((claim) => {
            if (claim.indexOf('role') === -1) {
                return;
            }
            result.push(token[claim]);
        });
        return result;
    }
}
