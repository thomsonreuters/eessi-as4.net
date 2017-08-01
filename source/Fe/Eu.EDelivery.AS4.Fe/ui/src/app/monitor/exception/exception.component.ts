import { FilterComponent } from './../filter/filter.component';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Observable } from 'rxjs';
import { Component, ViewChild } from '@angular/core';

import { Exception } from './../../api/Messages/Exception';
import { ExceptionService } from './exception.service';
import { ExceptionStore, IExceptionState } from './exception.store';
import { ExceptionFilter } from './exception.filter';
import { MESSAGESERVICETOKEN } from '../service.token';
import { timeRangeValidator } from '../../common/timeinput/timeinput.component';

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
    public exceptionFilterForm: FormGroup;
    @ViewChild('filter') public filter: FilterComponent;
    constructor(private _inExceptionStore: ExceptionStore, private _formBuilder: FormBuilder) {
        this.exceptionFilterForm = this._formBuilder.group({
            direction: [],
            operation: [],
            ebmsRefToMessageId: [],
            insertionTimeType: [0],
            insertionTimeFrom: [],
            insertionTimeTo: []
        }, { validator: timeRangeValidator('insertionTimeType', 'insertionTimeFrom', 'insertionTimeTo') });
        const filterValueChanges = this.exceptionFilterForm.valueChanges.subscribe((result) => this.inExceptionFilter = new ExceptionFilter(result));
        this.messages = this._inExceptionStore.changes;
        this.messages.subscribe((result) => console.log(result));
    }
    public toggle(message: Exception) {
        if (this.activeMessage === message) {
            this.activeMessage = undefined;
            return;
        }
        this.activeMessage = message;
    }
    public search() {
        this.filter.search();
    }
    public filtersLoaded(filter: ExceptionFilter) {
        this.exceptionFilterForm.reset(filter);
    }
    public clear(field: string) {
        this.exceptionFilterForm.get(field)!.setValue(null);
    }
}
