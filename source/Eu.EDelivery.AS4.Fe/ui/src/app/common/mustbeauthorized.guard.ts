import { CanActivate, Router, ActivatedRoute, ActivatedRouteSnapshot, RouterStateSnapshot } from '@angular/router';
import { Injectable } from '@angular/core';
import { tokenNotExpired, AuthHttp } from 'angular2-jwt';
import { Observable } from 'rxjs/Observable';

import { RolesService, Role } from './../authentication/roles.service';
import { AuthenticationService } from './../authentication/authentication.service';
import { DialogService } from './dialog.service';

@Injectable()
export class MustBeAuthorizedGuard implements CanActivate {
    constructor(private _router: Router, private _rolesService: RolesService, private _dialogService: DialogService, private _authenticationService: AuthenticationService) { }
    public isTokenValid(): boolean {
        return tokenNotExpired();
    }
    public canActivate(route: ActivatedRouteSnapshot, state: RouterStateSnapshot): Observable<boolean> {
        if (!this._authenticationService.isAuthenticated) {
            this._router.navigate(['/login']);
            return Observable.of(false);
        }

        // Get Route roles
        const roles = this.getData(route);
        if (!!roles) {
            // Validate that the user has the required roles
            const isValid = this._rolesService.validate(roles);
            if (!isValid) {
                this._router.navigate(['/unauthorized']);
            }

            return Observable.of(isValid);
        }

        return Observable.of(true);
    }
    private getData(route: ActivatedRouteSnapshot): string[] {
        if (!!route.firstChild) {
            return route.firstChild!.data['roles'];
        }
        return route.data['roles'];
    }
}
