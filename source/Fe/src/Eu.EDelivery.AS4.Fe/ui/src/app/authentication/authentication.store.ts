import { Injectable } from '@angular/core';

import { store, State } from './authentication.service';

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
