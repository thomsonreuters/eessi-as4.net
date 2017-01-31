import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { Directive, ElementRef } from '@angular/core';
import 'eonasdan-bootstrap-datetimepicker';
import * as moment from 'moment';
import * as $ from 'jquery';

@Directive({
    selector: '[as4-datetimepicker]',
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: DateTimePickerDirective,
            multi: true
        }
    ],
})
export class DateTimePickerDirective implements ControlValueAccessor {
    private _input: any;
    private _propagateChange: (_: Date) => {};
    private _control: any;
    constructor(private _elementRef: ElementRef) {
        this._control = $(this._elementRef.nativeElement).datetimepicker({
            format: 'DD/MM/YYYY HH:mm:ss'
        });
        this._control.on('dp.change', (data) => {
            if (!!!this._propagateChange) {
                return;
            }
            if (!!!data.date) {
                this._propagateChange(null);
                return;
            }
            this._propagateChange(data.date.toDate());
        });
    }
    public writeValue(value: Date) {
        if (!!!value) {
            return;
        }
        this._control.data('DateTimePicker').date(value);
    }
    public registerOnChange(fn) {
        this._propagateChange = fn;
    }
    public registerOnTouched() { }
    public setDisabledState(isDisabled: boolean) { }
}
