import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { PullRequestAuthorizationEntry } from './../../api/PullRequestAuthorizationEntry';

@Injectable()
export class AuthorizationMapService {
    private _data = new BehaviorSubject<PullRequestAuthorizationEntry[] | null>(null);
    constructor(private http: AuthHttp) { }
    public get(): Observable<PullRequestAuthorizationEntry[]> {
        return this
            .http
            .get(this.url())
            .map((result) => result.json());
    }
    public post(input: PullRequestAuthorizationEntry[]): Observable<boolean> {
        return this
            .http
            .post(this.url(), input)
            .map(() => true);
    }
    private url(): string {
        return 'api/configuration/authorizationmap';
    }
}
