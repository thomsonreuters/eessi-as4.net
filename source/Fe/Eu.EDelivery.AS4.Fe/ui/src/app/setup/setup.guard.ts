import { Observable } from 'rxjs/Observable';
import { Injectable } from '@angular/core';
import { ActivatedRouteSnapshot, CanActivate, RouterStateSnapshot, Router } from '@angular/router';

import { SetupService } from './setup.service';

@Injectable()
export class SetupGuard implements CanActivate {
    constructor(private _setupService: SetupService, private _router: Router) { }

    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        return this
            ._setupService
            .isSetup()
            .do((result) => {
                if (!result) {
                    this._router.navigate(['/setup']);
                }
            });
    }
}
