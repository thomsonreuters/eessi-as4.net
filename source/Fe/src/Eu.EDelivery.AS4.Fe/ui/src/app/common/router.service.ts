import { Observable } from 'rxjs/OBservable';
import { Injectable } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

import { flatten } from './utils';

@Injectable()
export class RouterService {
    constructor(private _location: Location, private _router: Router) {

    }

    public setCurrentValue(activatedRoute: ActivatedRoute, value: string, queryParams?: { }) {
        let route = flatten(activatedRoute
            .snapshot
            .pathFromRoot
            .filter((activeRoute) => !!activeRoute.url[0])
            .map((activeRoute) => activeRoute.url[0]))
            .join('/');

        // this._location.go(`${route}/${value}`, queryString);
        this._router.navigateByUrl(`${route}/${value}`, { queryParams: {} });
    }
}
