import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

import { Settings } from './../api/Settings';

const state = {
    Settings: new Settings()
}

interface State {
    Settings: Settings;
}

const store = new BehaviorSubject<any>(state);

@Injectable()
export class SettingsStore {
    private store = store;
    public changes = store.asObservable();
    public getState() {
        return this.store.value;
    }
    public setState(state: State) {
        this.store.next(state);
    }
}

@Injectable()
export class StoreHelper {
    constructor(private store: SettingsStore) { }
    update(prop, state) {
        const currentState = this.store.getState();
        this.store.setState(Object.assign({}, currentState, { [prop]: state }));
    }
    add(prop, state) {
        const currentState = this.store.getState();
        const collection = currentState[prop];
        this.store.setState(Object.assign({}, currentState, { [prop]: [state, ...collection] }));
    }
    findAndUpdate(prop, state) {
        const currentState = this.store.getState();
        const collection = currentState[prop];
        this.store.setState(Object.assign({}, currentState, {
        [prop]: collection.map(item => {
            if (item.id !== state.id) {
                return item;
            }
            return Object.assign({}, item, state)
        })
        }))
    }
    findAndDelete(prop, id) {
        const currentState = this.store.getState();
        const collection = currentState[prop];
        this.store.setState(Object.assign({}, currentState, { [prop]: collection.filter(item => item.id !== id) }));
    }
}
