import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';

function toBehaviorSubject<T>(defaultValue: any) {
    let subject = new BehaviorSubject(defaultValue);
    this.subscribe(subject);
    return subject;
}

Observable.prototype.toBehaviorSubject = toBehaviorSubject;

declare module 'rxjs/Observable' {
    interface Observable<T> {
        toBehaviorSubject<T>(defaultValue): BehaviorSubject<any>;
    }
}
