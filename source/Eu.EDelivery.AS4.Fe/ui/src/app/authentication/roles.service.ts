import { AuthenticationStore } from './authentication.store';
import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';
import { AuthHttp } from 'angular2-jwt';

export interface IRolesService {
    validate(role: string[]): boolean;
}

@Injectable()
export class RolesService implements IRolesService {
    private _baseUrl = '/api/roles';
    constructor(private _http: AuthHttp, private _authenticationStore: AuthenticationStore) { }
    public validate(roles: string[]): boolean {
        return !!this._authenticationStore.state.roles.find((search) => roles.findIndex((search2) => search2.toLocaleLowerCase() === search.toLocaleLowerCase()) !== -1);
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
    Admin = 'Admin',
    Readonly = 'Readonly'
}
