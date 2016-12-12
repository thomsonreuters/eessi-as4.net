import { DialogService } from './common/dialog.service';
import { ModalService } from './common/modal.service';
import { AuthenticationStore } from './authentication/authentication.service';
import { Component, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';

import { AppState } from './app.service';
import { RuntimeService } from './settings/runtime.service';

declare var $: any;

@Component({
    selector: 'as4-app',
    encapsulation: ViewEncapsulation.None,
    styles: [require('./app.component.scss').toString()],
    template: `        
        <router-outlet></router-outlet>
        <as4-modal name="default">
            </as4-modal>
        <as4-modal name="prompt" #promptDialog (shown)="input.focus(); promptDialog.result = ''">
            <input type="text" class="form-control" #input [value]="promptDialog.result" (keydown)="promptDialog.result = $event.target.value"/>             
        </as4-modal>
        <as4-modal name="error" showDefaultButtons="false" #errorDialog>
            <div *ngIf="isShowDetails" [class.stack-trace]="isShowDetails">
                <h3>Stack trace</h3>
                <button type="button" class="btn btn-outline" [ngIIclipboard]="payload" (cbOnSuccess)="copiedToClipboard()">Copy</button>
                <p #payload>{{errorDialog.payload}}</p>
            </div>
            <div buttons>
                <button type="button" class="btn btn-outline" (click)="showDetails()">Details</button>
                <button type="button" class="btn btn-outline" (click)="errorDialog.ok()">Ok</button>
            </div>
        </as4-modal>
  `
})
export class AppComponent {
    public isLoggedIn: boolean;
    public isShowDetails: boolean = false;
    @ViewChild('modal') modal: ElementRef;
    constructor(public appState: AppState, private authenticationStore: AuthenticationStore, private runtimeService: RuntimeService, private modalService: ModalService, private dialogService: DialogService) {
        this.authenticationStore.changes.subscribe(result => {
            this.isLoggedIn = result.loggedin;
            if (this.isLoggedIn) {
                this.runtimeService.getAll();
            }
        });
    }
    public showDetails() {
        this.isShowDetails = !this.isShowDetails;
    }
    public copiedToClipboard() {
        this.dialogService.message('Copied to clipboard');
    }
}
