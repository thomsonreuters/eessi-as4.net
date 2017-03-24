import { Observable } from 'rxjs';
import { Component } from '@angular/core';

import { Exception } from './../../api/Messages/Exception';
import { ExceptionService } from './exception.service';
import { ExceptionStore, IExceptionState } from './exception.store';
import { ExceptionFilter } from './exception.filter';
import { MESSAGESERVICETOKEN } from '../service.token';

@Component({
    selector: 'as4-in-exception',
    templateUrl: './exception.component.html',
    providers: [
        { provide: MESSAGESERVICETOKEN, useClass: ExceptionService }
    ]
})
export class ExceptionComponent {
    public messages: Observable<IExceptionState>;
    public activeMessage: Exception | undefined;
    public inExceptionFilter: ExceptionFilter = new ExceptionFilter();
    constructor(private _inExceptionStore: ExceptionStore) {
        this.messages = this._inExceptionStore.changes;
    }
    public toggle(message: Exception) {
        if (this.activeMessage === message) {
            this.activeMessage = undefined;
            return;
        }
        this.activeMessage = message;
    }
}
