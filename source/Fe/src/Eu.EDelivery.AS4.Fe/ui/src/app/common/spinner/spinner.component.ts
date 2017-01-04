import { SpinnerService } from './spinner.service';
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'as4-spinner',
    template: `<div class="loading" *ngIf="showSpinner">Loading&#8230;</div>`,
    styleUrls: ['./spinner.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SpinnerComponent {
    public showSpinner: boolean = false;
    constructor(private _spinnerService: SpinnerService) {
        this._spinnerService
            .changes
            .subscribe(result => {
                this.showSpinner = result;
            });
    }
}
