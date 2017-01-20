import { Subscription } from 'rxjs/Subscription';
import { Component, OnInit, Input, Output, forwardRef, ChangeDetectionStrategy } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';

import { PmodeStore } from '../pmode.store';
import { PMODECRUD_SERVICE } from './../crud/crud.component';

export const PMODESELECT_CONTROL_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => PmodeSelectComponent),
    multi: true
};

@Component({
    selector: 'as4-pmode-select',
    template: `
        <select class="form-control" (change)="selectPmode($event.target.value)" [disabled]="isDisabled">
            <option *ngFor="let pmode of pmodes" [selected]="pmode === selectedPmode">{{pmode}}</option>
        </select>
    `,
    providers: [PMODESELECT_CONTROL_VALUE_ACCESSOR]
})
export class PmodeSelectComponent implements OnInit, ControlValueAccessor {
    @Input() public mode: string;
    public selectedPmode: string;
    public pmodes: string[];
    public isDisabled: boolean;
    private _storeSubscription: Subscription;
    private _propagateChange: (_: string) => void;
    constructor(private pmodeStore: PmodeStore) { }
    public selectPmode(pmode: string) {
        this.selectedPmode = pmode;
        this._propagateChange(pmode);
    }
    public ngOnInit() {
        switch (this.mode) {
            case 'receiving':
                this._storeSubscription = this.pmodeStore.changes
                    .distinctUntilChanged()
                    .filter((result) => !!(result && result.ReceivingNames))
                    .map((result) => result.ReceivingNames)
                    .subscribe((result) => this.pmodes = result);
                break;
            case 'sending':
                this._storeSubscription = this.pmodeStore.changes
                    .distinctUntilChanged()
                    .filter((result) => !!(result && result.SendingNames))
                    .map((result) => result.SendingNames)
                    .subscribe((result) => this.pmodes = result);
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
