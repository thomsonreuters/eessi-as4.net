import { SpinnerService } from './spinner.service';
import { Component, OnInit, ChangeDetectionStrategy } from '@angular/core';

@Component({
    selector: 'as4-spinner',
    template: `<div class="overlay" *ngIf="showSpinner"><div class="spinner-container"><div class="spinner"></div></div></div>`,
    styleUrls: ['./spinner.component.scss']
})
export class SpinnerComponent {
    public showSpinner: boolean = false;
    constructor(private _spinnerService: SpinnerService) {
        this._spinnerService
            .changes
            .debounceTime(100)
            .subscribe(result => {
                this.showSpinner = result;
            });
    }
}
