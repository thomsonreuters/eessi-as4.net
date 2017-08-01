import { Subscription } from 'rxjs/Subscription';
import { NgForm, FormGroup } from '@angular/forms';
import { Component, ChangeDetectionStrategy, ChangeDetectorRef, Input, EventEmitter, Output, SkipSelf, OnInit, OnDestroy } from '@angular/core';

@Component({
    selector: 'as4-timeinput',
    templateUrl: 'timeinput.component.html',
    styleUrls: ['./timeinput.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TimeInputComponent implements OnInit, OnDestroy {
    @Input() public input: FormGroup;
    @Input() public type: string;
    @Input() public from: string;
    @Input() public to: string;
    private _subscription: Subscription;
    constructor(private _changeDetectorRef: ChangeDetectorRef) { }
    public ngOnInit() {
        this._subscription = this.input
            .get(this.type)!
            .valueChanges
            .subscribe((result) => {
                if (result !== '5') {
                    this.input.patchValue({
                        [this.from]: null,
                        [this.to]: null
                    });
                }
            });
    }
    public ngOnDestroy() {
        if (!!this._subscription) {
            this._subscription.unsubscribe();
        }
    }
    public selectedType(): number {
        return +this.input.get(this.type)!.value;
    }
}

export function timeRangeValidator(type: string, from: string, to: string) {
    return (group: FormGroup) => {
        const typeControl = group.get(type)!.value;
        const fromValue = group.get(from);
        const toValue = group.get(to);

        // tslint:disable-next-line:max-line-length
        if (typeControl === '5' && ((!!!fromValue!.value && !!!toValue!.value) || (!!toValue!.value && !!fromValue!.value && toValue!.value < fromValue!.value))) {
            fromValue!.setErrors({ required: true });
            toValue!.setErrors({ required: true });
        } else {
            fromValue!.setErrors(null);
            toValue!.setErrors(null);
        }
    };
}