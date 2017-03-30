import { DialogService } from './dialog.service';
import { RouterModule, Router } from '@angular/router';
import { FormBuilder } from '@angular/forms';
import { Observer } from 'rxjs/Observer';
import { Observable } from 'rxjs/Observable';
import {
    inject,
    fakeAsync,
    TestBed
} from '@angular/core/testing';
import { Component } from '@angular/core';
import {
    BaseRequestOptions,
    ConnectionBackend,
    Http
} from '@angular/http';
import { MockBackend } from '@angular/http/testing';
import { RouterTestingModule } from '@angular/router/testing';

import { MustBeAuthorizedGuard } from './mustbeauthorized.guard';
import { Role, RolesService, IRolesService } from '../authentication/roles.service';

class RolesServiceMock implements IRolesService {
    public validate(path: string): Observable<Role> {
        return Observable.of(Role.Read);
    }
}

class RouterMock {
    navigate(commands: any[]) {

    }
}

class DialogServiceMock {
    message(msg: string) {

    }
}

describe('mustbeauthorized guard', () => {
    beforeEach(() => TestBed.configureTestingModule({
        providers: [
            MustBeAuthorizedGuard,
            { provide: RolesService, useClass: RolesServiceMock },
            { provide: Router, useClass: RouterMock },
            { provide: DialogService, useClass: DialogServiceMock }
        ],
        imports: [

        ]
    }));
    it('should call rolesservice.validate', inject([MustBeAuthorizedGuard, RolesService], fakeAsync((guard: MustBeAuthorizedGuard, service: RolesService) => {
        let spy = spyOn(service, 'validate').and.returnValue(Observable.of(Role.Read));
        guard.canActivate()
            .subscribe((authorized) => {
                expect(spy).toHaveBeenCalled();
            });
    })));
    it('should return false when user doesn\'t have read role and display a message', inject([MustBeAuthorizedGuard, RolesService, DialogService], fakeAsync((guard: MustBeAuthorizedGuard, service: RolesService, dialogService: DialogService) => {
        let serviceSpy = spyOn(service, 'validate').and.returnValue(Observable.of(null));
        let dialogSpy = spyOn(dialogService, 'message');
        guard.canActivate().subscribe((authorized) => {
            expect(serviceSpy).toHaveBeenCalled();
            expect(authorized).toBeFalsy();
            expect(dialogSpy).toHaveBeenCalled();
        });
    })));
    it('should return true when the user has access', inject([MustBeAuthorizedGuard, RolesService], fakeAsync((guard: MustBeAuthorizedGuard, service: RolesService) => {
        let serviceSpy = spyOn(service, 'validate').and.returnValue(Observable.of(Role.Read));
        spyOn(guard, 'isTokenValid').and.returnValue(true);
        guard.canActivate().subscribe((authorized) => {
            expect(authorized).toBeTruthy();
        });
    })));
    it('should navigate back to login when token is expired', inject([MustBeAuthorizedGuard, RolesService, Router], fakeAsync((guard: MustBeAuthorizedGuard, service: RolesService, router: Router) => {
        let serviceSpy = spyOn(service, 'validate').and.returnValue(Observable.of(Role.Read));
        spyOn(guard, 'isTokenValid').and.returnValue(false);
        let routeSpy = spyOn(router, 'navigate');
        guard.canActivate().subscribe((authorized) => {
            expect(authorized).toBeFalsy();
            expect(routeSpy).toHaveBeenCalledWith(['login']);
        });
    })));
});
