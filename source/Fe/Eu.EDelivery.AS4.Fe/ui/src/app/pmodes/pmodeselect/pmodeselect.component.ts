import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';
import { Component, Input, Output, forwardRef, ChangeDetectionStrategy, OnDestroy, OnInit, ChangeDetectorRef, ViewRef } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR, NgControl } from '@angular/forms';

import { PmodeStore } from '../pmode.store';
import { PMODECRUD_SERVICE } from './../crud/crud.component';

@Component({
    selector: 'as4-pmode-select, [as4-pmode-select]',
    template: `
        <select class="form-control" (change)="selectPmode($event.target.value)" [attr.disabled]="!isDisabled ? null : true" *ngIf="!multi">
            <option value="null">None</option>
            <option *ngFor="let pmode of pmodes" [selected]="pmode === selectedPmode">{{pmode}}</option>
        </select>
        <select class="form-control" [ngModel]="selectedPmode" (ngModelChange)="selectPmode($event)" [attr.disabled]="!isDisabled ? null : true" multiselect="false" *ngIf="multi">
            <option *ngFor="let pmode of pmodes">{{pmode}}</option>
        </select>
    `,
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: PmodeSelectComponent,
        multi: true
    }]
})
export class PmodeSelectComponent implements OnInit, OnDestroy, ControlValueAccessor {
    @Input() public mode: string | null;
    @Input() public selectFirst: boolean = false;
    @Input() public multi: boolean = false;
    public isDisabled: boolean;
    public selectedPmode: string | string[] | null;
    public pmodes: string[] | undefined;
    private _storeSubscription: Subscription;
    private _propagateChange: (_: string | null) => void | undefined;
    constructor(private pmodeStore: PmodeStore) { }
    public selectPmode(pmode: string | null = null) {
        pmode = pmode === 'null' ? null : pmode;
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
                        if (!!!result) {
                            return;
                        }
                        if (this.selectFirst) {
                            setTimeout(() => this.selectPmode(result[0]));
                        }
                    });
                break;
            default:
                let receivingPmodes = this.pmodeStore.changes.distinctUntilChanged().filter((result) => !!(result && result.ReceivingNames)).map((result) => result.ReceivingNames);
                let sendingPmodes = this.pmodeStore.changes.distinctUntilChanged().filter((result) => !!(result && result.SendingNames)).map((result) => result.SendingNames);
                this._storeSubscription = Observable
                    .combineLatest(receivingPmodes, sendingPmodes)
                    .filter((result) => !!result[0] && !!result[1])
                    .subscribe((result) => {
                        this.pmodes = result[0]!.concat(result[1]!);
                        if (!!!result) {
                            return;
                        }
                        if (this.selectFirst) {
                            setTimeout(() => this.selectPmode(this.pmodes![0]));
                        }
                    });
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
