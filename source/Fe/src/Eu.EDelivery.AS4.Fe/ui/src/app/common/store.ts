import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

export class Store<T> {
    public state: T;
    protected storeSubject: BehaviorSubject<T> = new BehaviorSubject<T>(this.state);
    public changes = this.storeSubject.asObservable();
    constructor(state: T) {
        this.state = state;
    }
    public getState() {
        return this.storeSubject.value;
    }
    public setState(state: T) {
        this.state = state;
        this.storeSubject.next(state);
    }
    update(prop, state) {
        this.setState(Object.assign({}, this.state, { [prop]: state }));
    }
    add(prop, state) {
        const collection = state[prop];
        this.setState(Object.assign({}, this.state, { [prop]: [state, ...collection] }));
    }
    findAndUpdate(prop, state) {
        const collection = this.state[prop];
        this.setState(Object.assign({}, this.state, {
            [prop]: collection.map(item => {
                if (item.id !== state.id) {
                    return item;
                }
                return Object.assign({}, item, state);
            })
        }));
    }
    findAndDelete(prop, id) {
        const collection = this.state[prop];
        this.setState(Object.assign({}, this.state, { [prop]: collection.filter(item => item.id !== id) }));
    }
}
