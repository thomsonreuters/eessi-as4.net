import { ActivatedRoute } from '@angular/router';
import { Component } from '@angular/core';

import { ExceptionService } from './../exception/exception.service';

@Component({
    selector: 'as4-exceptiondetail',
    templateUrl: 'exceptiondetail.component.html'
})

export class ExceptionDetailComponent {
    public data: string;
    constructor(private _activatedRoute: ActivatedRoute, private _exceptionService: ExceptionService) {
        let direction = this._activatedRoute.snapshot.params['direction'];
        let id = this._activatedRoute.snapshot.params['messageid'];

        this._exceptionService
            .getDetail(direction, id)
            .subscribe((result) => this.data = result);
    }
}
