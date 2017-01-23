import { Injectable } from '@angular/core';
import { JwtHelper, tokenNotExpired } from 'angular2-jwt';
import { Observable, BehaviorSubject } from 'rxjs';

const state = {
    loggedin: tokenNotExpired()
};

export interface State {
    loggedin: boolean;
}
export const store = new BehaviorSubject<State>(state);

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

export const TOKENSTORE: string = 'id_token';
