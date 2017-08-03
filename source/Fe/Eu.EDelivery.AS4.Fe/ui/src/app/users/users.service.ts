import { AuthHttp } from 'angular2-jwt';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

@Injectable()
export class UsersService {
    constructor(private http: AuthHttp) { }
    public get(): Observable<User[]> {
        return this
            .http
            .get(this.getUrl())
            .map((result) => result.json());
    }
    public create(user: User): Observable<boolean> {
        return this
            .http
            .post(this.getUrl(), user)
            .map(() => true);
    }
    public delete(user: string): Observable<boolean> {
        return this
            .http
            .delete(this.getUrl() + `/${user}`)
            .map(() => true);
    }
    public update(user: User): Observable<boolean> {
        return this
            .http
            .put(this.getUrl() + `/${user.name}`, user)
            .map(() => true);
    }
    private getUrl(): string {
        return `/api/user`;
    }
}

// tslint:disable-next-line:max-classes-per-file
export class User {
    public name: string;
    public password: string;
    public isAdmin: boolean;
}
