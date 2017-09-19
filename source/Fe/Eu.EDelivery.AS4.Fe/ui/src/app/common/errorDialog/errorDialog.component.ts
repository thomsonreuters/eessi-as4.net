import { ModalComponent } from './../modal/modal.component';
import { Component, ViewChild, OnInit } from '@angular/core';

@Component({
    selector: 'as4-error',
    template: `
        <as4-modal showDefaultButtons="false" #errorDialog>
            <div *ngIf="isShowDetails && !!errorDialog.payload" [class.stack-trace]="isShowDetails">
                <h4>Stack trace</h4>
                <i class="fa fa-clipboard clickable" [ngxClipboard]="payload" (cbOnSuccess)="tooltip.show()" as4-tooltip="Copied to clipboard" as4-tooltip-manual="true" #tooltip="as4-tooltip"></i>
                <p #payload>{{errorDialog.payload}}</p>
            </div>
            <div buttons>
                <button type="button" class="btn" *ngIf="!!errorDialog.payload" (click)="isShowDetails = !!!isShowDetails ? true : isShowDetails">DETAILS</button>
                <button type="button" class="btn" *ngIf="showOk" (click)="errorDialog.ok()" focus>OK</button>
            </div>
        </as4-modal>`
})

export class ErrorDialogComponent implements OnInit {
    @ViewChild(ModalComponent) public modal: ModalComponent;
    public ngOnInit() {
        this.modal.isVisible = true;
    }
}
