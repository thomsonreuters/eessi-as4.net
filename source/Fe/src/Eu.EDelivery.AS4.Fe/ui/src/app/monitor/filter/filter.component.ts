import { ActivatedRoute, ActivatedRouteSnapshot, Router } from '@angular/router';
import { Component, OnInit, OpaqueToken, Inject, EventEmitter, Output, Input } from '@angular/core';

import { BaseFilter } from './../base.filter';
import { IMessageService, MESSAGESERVICETOKEN } from '../messageservice.interface';

@Component({
    selector: 'as4-filter',
    templateUrl: './filter.component.html'
})
export class FilterComponent implements OnInit {
    @Input() public filter: BaseFilter;
    @Output() public onSearch: EventEmitter<BaseFilter> = new EventEmitter();
    constructor( @Inject(MESSAGESERVICETOKEN) private _messageService: IMessageService, private _activatedRoute: ActivatedRoute, private _router: Router) {

    }
    public ngOnInit() {
        this._messageService.getMessages(this.filter.fromUrlParams(this._activatedRoute.snapshot.queryParams));
    }
    public search(resetPage: boolean = false) {
        if (resetPage) {
            this.filter.page = 1;
        }
        this._router.navigate(this.getPath(this._activatedRoute), { queryParams: this.filter.sanitize() });
        this._messageService.getMessages(this.filter);
    }
    private getPath(route: ActivatedRoute): string[] {
        let path = new Array<string>();
        let test: ActivatedRouteSnapshot = route.snapshot;
        do {
            path.push(test.url.toString());
            test = test.parent;
        }
        while (!!test && test.url.length > 0);
        return path.reverse();
    }
}
