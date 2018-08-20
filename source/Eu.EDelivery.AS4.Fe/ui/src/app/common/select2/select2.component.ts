import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';
import { Subscription } from 'rxjs/Subscription';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { NG_VALUE_ACCESSOR, ControlValueAccessor } from '@angular/forms';
import { Component, ElementRef, ViewChild, Input, Output, EventEmitter, ViewEncapsulation, AfterViewInit, OnDestroy, ChangeDetectionStrategy } from '@angular/core';

import * as $ from 'jquery';

@Component({
    selector: 'as4-select2',
    template: `<select #select></select>`,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: Select2Component,
            multi: true
        }
    ],
    styles: [`
        .select2-dropdown {
            border-radius: 0 !important;
        }
        .select2-container--open {
            z-index: 2000000;
        }
    `],
    encapsulation: ViewEncapsulation.None,
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class Select2Component implements ControlValueAccessor, OnDestroy {
    @Input() public set data(input: Select2Data[]) {
        this._hasData.next(input);
    }
    @Input() public open: any | undefined;
    @Output() public change: EventEmitter<Select2Data> = new EventEmitter<Select2Data>();
    @ViewChild('select') private _select: ElementRef;
    private _selectel: any;
    private _propagateChange: (_: any) => {};

    private _subscription: Subscription;
    private _hasData: Subject<Select2Data[]> = new Subject();
    private _selectedValue: BehaviorSubject<number | string | null> = new BehaviorSubject(null);
    private _isInited: BehaviorSubject<boolean> = new BehaviorSubject(false);
    constructor() {
        this._subscription = Observable
            .combineLatest(this._isInited, this._hasData, this._selectedValue)
            .filter(result => !!result[0] && !!result[1])
            .subscribe((result) => {
                this._selectel.select2().empty();
                this._selectel.select2({ data: result[1] });
                this._selectel.val([result[2]]).trigger('change');
            });
    }
    public ngAfterViewInit() {
        this._selectel = $(this._select.nativeElement).select2({
            width: '100%',
            multiple: false
        })
            .on('select2:select select2:unselect', (e) => {
                let selectedData = this._selectel.select2('data');
                if (!!!selectedData || !!!selectedData[0]) {
                    return;
                }

                let changedData = {
                    id: selectedData[0].id,
                    text: selectedData[0].text
                };
                if (!!this._propagateChange) {
                    this._propagateChange(changedData);
                }
                this.change.emit(changedData);
                this.setFocus();
            });
        this._isInited.next(true);
        if (this.open !== undefined ) this.setFocus();
    }
    public ngOnDestroy() {
        this._subscription.unsubscribe();
        $(this._select.nativeElement).select2('destroy');
    }
    public writeValue(value: number | string | null) {
        this._selectedValue.next(value);
    }
    public registerOnChange(fn) {
        this._propagateChange = fn;
    }
    public registerOnTouched() { }
    private setFocus() {
        setTimeout(() => $(this._select.nativeElement).next().find('.select2-selection').focus());
    }
}

export class Select2Data {
    id: number | string;
    text: string;
    constructor(input: Partial<Select2Data>) {
        Object.assign(this, input);
    }
}