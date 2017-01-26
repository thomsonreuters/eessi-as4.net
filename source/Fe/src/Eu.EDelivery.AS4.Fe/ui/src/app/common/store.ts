import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable }   from 'rxjs/OBservable';

export class Store<T> {
    public state: T;
    public changes: Observable<T>;
    protected storeSubject: BehaviorSubject<T>;
    constructor(state: T) {
        this.storeSubject = new BehaviorSubject<T>(state);
        this.changes = this.storeSubject.asObservable();
        this.state = state;
    }
    public getState() {
        return this.storeSubject.value;
    }
    public setState(state: T) {
        this.state = state;
        this.storeSubject.next(state);
    }
    public update(prop, state) {
        this.setState(Object.assign({}, this.state, { [prop]: state }));
    }
    public add(prop, state) {
        const collection = state[prop];
        this.setState(Object.assign({}, this.state, { [prop]: [state, ...collection] }));
    }
    public findAndUpdate(prop, state) {
        const collection = this.state[prop];
        this.setState(Object.assign({}, this.state, {
            [prop]: collection.map((item) => {
                if (item.id !== state.id) {
                    return item;
                }
                return Object.assign({}, item, state);
            })
        }));
    }
    public findAndDelete(prop, id) {
        const collection = this.state[prop];
        this.setState(Object.assign({}, this.state, { [prop]: collection.filter((item) => item.id !== id) }));
    }
}
