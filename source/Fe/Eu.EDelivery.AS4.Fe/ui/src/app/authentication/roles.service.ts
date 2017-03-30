import { AuthenticationStore } from './authentication.store';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';

export interface IRolesService {
    validate(path: string): Observable<Role>;
}

@Injectable()
export class RolesService implements IRolesService {
    private _baseUrl = '/api/roles';
    constructor(private _http: AuthHttp, private _authenticationStore: AuthenticationStore) {

    }
    public validate(path: string): Observable<Role> {
        return Observable.of(Role.Read | Role.Write);
    }
    private getUrl(action?: string): string {
        if (!!!action) {
            return this._baseUrl;
        } else {
            return `${this._baseUrl}/${action}`;
        }
    }
}

export enum Role {
    Read = 1 << 0,
    Write = 1 << 1
}
