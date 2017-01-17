import { NG_VALUE_ACCESSOR, ControlValueAccessor, NG_VALIDATORS, FormControl, FormControlDirective, ControlContainer, FormControlName, NgControl, FormGroup, AbstractControl } from '@angular/forms';
import { Component, OnInit, Input, Host, Inject, Optional, SkipSelf, forwardRef, EventEmitter } from '@angular/core';

@Component({
    selector: 'as4-thumbprint-input',
    template: `<div class="input-group col-md-12">
        <input type="text" [disabled]="isDisabled" [(ngModel)]="input" [ngClass]="{ 'ng-invalid': !!errors && !!!errors['required'] && !!errors['validThumbPrint'] }"/>
        <span class="input-group-btn" *ngIf="!!errors && !!!errors['required'] && !!errors['validThumbPrint']">
            <button class="btn btn-danger" (click)="sanitize()" [disabled]="isDisabled" as4-tooltip="The input contains invalid characters, you can press this button to sanitize it"><i class="fa fa-check"></i></button>
        </span>
    </div>
    `,
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: ThumbprintInputComponent,
            multi: true
        }
    ],
    styleUrls: ['./thumbprintInput.component.scss']
})
export class ThumbprintInputComponent implements ControlValueAccessor {
    private static charReg = new RegExp(/[0-9a-fA-F]/);
    public isDisabled: boolean;
    public get input(): string {
        return this._input;
    }
    public name: EventEmitter<string>;
    public set input(value: string) {
        this._input = value;
        if (!!!this._propagateChange) return;
        this._propagateChange(this.input);
    }
    public get errors() {
        return this.formControl.control.get(this.formControlName).errors;
    }
    @Input() formControlName: string;
    private _input: string;
    private _propagateChange: (_: string) => void;
    constructor( @SkipSelf() @Host() @Inject(forwardRef(() => ControlContainer)) private formControl: ControlContainer) {
    }
    public sanitize() {
        let charReg = new RegExp(/[0-9a-fA-F]/);
        let newString = '';
        for (let cur = 0; cur < this.input.length; cur++) {
            if (ThumbprintInputComponent.charReg.test(this.input[cur])) {
                newString += this.input[cur];
            }
        }
        this.input = newString;
    }
    public writeValue(value: string) {
        this._input = value;
    }
    public registerOnChange(fn) {
        this._propagateChange = fn;
    }
    public registerOnTouched() {

    }
    setDisabledState(isDisabled: boolean) {
        this.isDisabled = isDisabled;
    }
}
