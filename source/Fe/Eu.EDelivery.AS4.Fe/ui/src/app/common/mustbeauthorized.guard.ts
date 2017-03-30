import { DialogService } from './dialog.service';
import { CanActivate, Router } from '@angular/router';
import { Injectable } from '@angular/core';
import { tokenNotExpired, AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { RolesService, Role } from './../authentication/roles.service';

@Injectable()
export class MustBeAuthorizedGuard implements CanActivate {
    constructor(private _router: Router, private _rolesService: RolesService, private _dialogService: DialogService) {

    }
    public isTokenValid(): boolean {
        return tokenNotExpired();
    }
    public canActivate(): Observable<boolean> {
        return Observable.create((obs) => {
            this._rolesService
                .validate('/pmodes/receiving')
                .map((result) => result & Role.Read)
                .subscribe((result) => {
                    if (!result) {
                        this._dialogService.message('No access to this route');
                        obs.next(false);
                        obs.complete();
                        return;
                    }

                    if (this.isTokenValid()) {
                        obs.next(true);
                        obs.complete();
                        return true;
                    }

                    this._router.navigate(['login']);

                    obs.next(false);
                    obs.complete(true);
                });
        });
    }
}
