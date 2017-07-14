import { Subscription } from 'rxjs/Subscription';
import { MESSAGESERVICETOKEN } from './../service.token';
import { MessageFilter } from './../message/message.filter';
import { ActivatedRoute, ActivatedRouteSnapshot, Router } from '@angular/router';
import {
    Component,
    OnInit,
    OpaqueToken,
    Inject,
    EventEmitter,
    Output,
    Input,
    ChangeDetectionStrategy,
    OnDestroy
} from '@angular/core';

import { BaseFilter } from './../base.filter';
import { MessageService } from '../message/message.service';

@Component({
    selector: 'as4-filter',
    templateUrl: './filter.component.html'
})
export class FilterComponent implements OnInit, OnDestroy {
    @Input() public filter: MessageFilter;
    @Output() public outFilter: MessageFilter;
    @Output() public onSearch: EventEmitter<BaseFilter> = new EventEmitter();
    private _subscriptions: Subscription[] = new Array<Subscription>();
    // tslint:disable-next-line:max-line-length
    constructor( @Inject(MESSAGESERVICETOKEN) private _messageService: MessageService, private _activatedRoute: ActivatedRoute, private _router: Router) {
        let routeChangeSub = this._activatedRoute
            .queryParams
            .filter(() => !!this.filter)
            .subscribe((result) => this.executeSearch());
        this._subscriptions.push(routeChangeSub);
    }
    public ngOnInit() {
        this.executeSearch();
    }
    public ngOnDestroy() {
        this._subscriptions.forEach((sub) => sub.unsubscribe());
    }
    public search(resetPage: boolean = false) {
        if (resetPage) {
            this.filter.page = 1;
        }
        this._router
            .navigate(this.getPath(this._activatedRoute), { queryParams: this.filter.sanitize() })
            .then((result) => {
                if (result === null) {
                    // When nothing happened call the service manually, this is usually the case when the queryparams haven't changed.
                    this.executeSearch();
                }
            });
    }
    public executeSearch() {
        this.queryParamsToFilter();
        this._messageService.getMessages(this.filter);
    }
    private getPath(route: ActivatedRoute): string[] {
        let path = new Array<string>();
        let test: ActivatedRouteSnapshot | null = route.snapshot;
        do {
            path.push(test.url.toString());
            test = test.parent;
        }
        while (!!test && test.url.length > 0);
        return path.reverse();
    }
    private queryParamsToFilter() {
        this.filter.fromUrlParams(this._activatedRoute.snapshot.queryParams);
        this.outFilter = Object.assign({}, this.filter);
    }
}
