import { Observable } from 'rxjs/OBservable';
import { Injectable } from '@angular/core';
import { Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';

import { flatten } from './utils';

@Injectable()
export class RouterService {
    constructor(private _location: Location, private _router: Router) { }
    // tslint:disable-next-line:max-line-length
    public setCurrentValue(activatedRoute: ActivatedRoute, value: string, queryParams?: {}, useReplaceState: boolean = true) {
        let route = flatten(activatedRoute
            .snapshot
            .pathFromRoot
            .filter((activeRoute) => !!activeRoute.url[0])
            .map((activeRoute) => activeRoute.url[0]))
            .join('/');

        if (useReplaceState) {
            this._location.replaceState(`${route}/${value}`);
        } else {
            this._router.navigateByUrl(`${route}/${value}`, { queryParams: {} });
        }
    }
}
