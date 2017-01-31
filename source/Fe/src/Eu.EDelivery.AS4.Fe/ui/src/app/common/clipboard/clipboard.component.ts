import { DialogService } from './../dialog.service';
import { Component, OnInit, Input, ElementRef } from '@angular/core';

@Component({
    selector: 'as4-clipboard',
    template: `
       <i class="fa fa-clipboard" [ngxClipboard]="target" (cbOnSuccess)="copiedToClipboard()"></i>
   `
})
export class ClipboardComponent {
    @Input() public message: string = 'Copied to clipboard';
    @Input() public target: ElementRef;
    constructor(private _dialogService: DialogService) {

    }
    public copiedToClipboard() {
        this._dialogService.message(this.message);
    }
}
