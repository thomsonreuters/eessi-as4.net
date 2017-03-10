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
    constructor( @Inject(MESSAGESERVICETOKEN) private _messageService: MessageService, private _activatedRoute: ActivatedRoute, private _router: Router) {
        this._subscriptions.push(this._activatedRoute
            .queryParams
            .filter(() => !!this.filter)
            .subscribe((result) => {
                this.executeServiceCall();
            }));
    }
    public ngOnInit() {
        this.executeServiceCall();
    }
    public ngOnDestroy() {
        this._subscriptions.forEach((sub) => sub.unsubscribe());
    }
    public search(resetPage: boolean = false) {
        if (resetPage) {
            this.filter.page = 1;
        }
        this._router.navigate(this.getPath(this._activatedRoute), { queryParams: this.filter.sanitize() });
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
    private queryParamsToFilter() {
        this.filter.fromUrlParams(this._activatedRoute.snapshot.queryParams);
        this.outFilter = Object.assign({}, this.filter);
    }
    private executeServiceCall() {
        this.queryParamsToFilter();
        this._messageService.getMessages(this.filter);
    }
}
