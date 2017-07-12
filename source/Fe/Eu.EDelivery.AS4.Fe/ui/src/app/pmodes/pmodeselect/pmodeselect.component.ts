import { Subscription } from 'rxjs/Subscription';
import { Component, Input, Output, forwardRef, ChangeDetectionStrategy, OnDestroy, OnInit, ChangeDetectorRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, NgControl } from '@angular/forms';

import { PmodeStore } from '../pmode.store';
import { PMODECRUD_SERVICE } from './../crud/crud.component';

@Component({
    selector: 'as4-pmode-select, [as4-pmode-select]',
    template: `
        <select class="form-control" (change)="selectPmode($event.target.value)" [attr.disabled]="!isDisabled ? null : true">
            <option value="null">None</option>
            <option *ngFor="let pmode of pmodes" [selected]="pmode === selectedPmode">{{pmode}}</option>
        </select>
    `,
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: PmodeSelectComponent,
        multi: true
    }]
})
export class PmodeSelectComponent implements OnInit, OnDestroy, ControlValueAccessor {
    @Input() public mode: string;
    @Input() public selectFirst: boolean = false;
    public isDisabled: boolean;
    public selectedPmode: string | null;
    public pmodes: string[] | undefined;
    private _storeSubscription: Subscription;
    private _propagateChange: (_: string | null) => void | undefined;
    constructor(private pmodeStore: PmodeStore, private _changeDetectorRef: ChangeDetectorRef) {    }
    public selectPmode(pmode: string | null = null) {
        this.selectedPmode = pmode;
        if (!!this._propagateChange) {
            this._propagateChange(pmode);
        }
    }
    public ngOnInit() {
        switch (this.mode) {
            case 'receiving':
                this._storeSubscription = this.pmodeStore.changes
                    .distinctUntilChanged()
                    .filter((result) => !!(result && result.ReceivingNames))
                    .map((result) => result.ReceivingNames)
                    .subscribe((result) => {
                        this.pmodes = result;
                        this._changeDetectorRef.detectChanges();
                        if (!!!result) {
                            return;
                        }
                        if (this.selectFirst) {
                            setTimeout(() => this.selectPmode(result[0]));
                        }
                    });
                break;
            case 'sending':
                this._storeSubscription = this.pmodeStore.changes
                    .distinctUntilChanged()
                    .filter((result) => !!(result && result.SendingNames))
                    .map((result) => result.SendingNames)
                    .subscribe((result) => {
                        this.pmodes = result;
                        this._changeDetectorRef.detectChanges();
                        if (!!!result) {
                            return;
                        }
                        if (this.selectFirst) {
                            setTimeout(() => this.selectPmode(result[0]));
                        }
                    });
                break;
            default:
                throw Error('Mode should be supplied');
        }
    }
    public ngOnDestroy() {
        this._storeSubscription.unsubscribe();
    }
    public writeValue(value: string) {
        this.selectedPmode = value;
    }
    public registerOnChange(fn) {
        this._propagateChange = fn;
    }
    public registerOnTouched() { }
    public setDisabledState(isDisabled: boolean): void {
        this.isDisabled = isDisabled;
    }
}
