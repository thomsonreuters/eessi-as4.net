import { MESSAGESERVICETOKEN } from './../messageservice.interface';
import { ActivatedRoute, Router, ActivatedRouteSnapshot } from '@angular/router';
import { Observable } from 'rxjs';
import { Component, trigger, style, transition, state, animate, EventEmitter, Output } from '@angular/core';

import { InException } from './../../api/Messages/InException';
import { InExceptionService } from './inexception.service';
import { InExceptionFilter } from './inexception.filter';
import { InExceptionStore, IInExceptionState } from './inexception.store';
import { DialogService } from './../../common/dialog.service';

@Component({
    selector: 'as4-in-exception',
    templateUrl: './inexception.component.html',
    providers: [
        { provide: MESSAGESERVICETOKEN, useClass: InExceptionService }
    ]
})
export class InExceptionComponent {
    public messages: Observable<IInExceptionState>;
    public activeMessage: InException | undefined;
    public inExceptionFilter: InExceptionFilter = new InExceptionFilter();
    constructor(private _inExceptionStore: InExceptionStore) {
        this.messages = this._inExceptionStore.changes;
    }
    public toggle(message: InException) {
        if (this.activeMessage === message) {
            this.activeMessage = undefined;
            return;
        }
        this.activeMessage = message;
    }
    public test(column: string) {
alert(column);
    }
}
