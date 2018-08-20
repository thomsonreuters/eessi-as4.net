import { DialogService } from './dialog.service';
import { Injectable } from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { Observable } from 'rxjs/Observable';

export interface CanComponentDeactivate {
    canDeactivate: () => Observable<boolean> | Promise<boolean> | boolean;
}

@Injectable()
export class CanDeactivateGuard implements CanDeactivate<CanComponentDeactivate> {
    constructor(private _dialogService: DialogService) {

    }
    public canDeactivate(component: CanComponentDeactivate): Observable<boolean> | boolean {
        if (!!!component) {
            return true;
        }
        let result = component.canDeactivate ? component.canDeactivate() : true;
        if (!result) {
            return this._dialogService.confirm('You have unsaved changes are you sure you want to leave ?');
        } else {
            return true;
        }
    }
}
