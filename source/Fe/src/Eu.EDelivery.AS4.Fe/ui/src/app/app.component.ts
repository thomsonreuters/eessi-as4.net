import { SettingsService } from './settings/settings.service';
import { DialogService } from './common/dialog.service';
import { ModalService } from './common/modal/modal.service';
import { Component, ViewEncapsulation, ViewChild, ElementRef } from '@angular/core';

import { AppState } from './app.service';
import { RuntimeService } from './settings/runtime.service';
import { AuthenticationStore } from './authentication/authentication.store';
import { SendingPmodeService } from './pmodes/sendingpmode.service';
import { ReceivingPmodeService } from './pmodes/receivingpmode.service';

import 'jquery';
import 'bootstrap/dist/js/bootstrap.js';
import '../theme/js/app.js';

@Component({
    selector: 'as4-app',
    encapsulation: ViewEncapsulation.None,
    styles: [
        './app.component.scss'
    ],
    template: `    
        <as4-modal name="default"></as4-modal>
        <as4-modal name="prompt" #promptDialog (shown)="input.focus(); promptDialog.result = ''">
            <input type="text" class="form-control" #input [value]="promptDialog.result" (keyup)="promptDialog.result = $event.target.value"/>             
        </as4-modal>
        <as4-modal name="error" showDefaultButtons="false" #errorDialog>
            <div *ngIf="isShowDetails && !!errorDialog.payload" [class.stack-trace]="isShowDetails">
                <h3>Stack trace</h3>
                <button type="button" class="btn btn-outline" [ngxClipboard]="payload" (cbOnSuccess)="copiedToClipboard()">Copy</button>
                <p #payload>{{errorDialog.payload}}</p>
            </div>
            <div buttons>
                <button type="button" class="btn btn-outline" *ngIf="!!errorDialog.payload" (click)="showDetails()">Details</button>
                <button type="button" class="btn btn-outline" (click)="errorDialog.ok()">Ok</button>
            </div>
        </as4-modal>
        <as4-spinner></as4-spinner>
        <router-outlet></router-outlet>        
  `
})
export class AppComponent {
    public isLoggedIn: boolean;
    public isShowDetails: boolean = false;
    @ViewChild('modal') public modal: ElementRef;
    constructor(public appState: AppState, private authenticationStore: AuthenticationStore, private runtimeService: RuntimeService, private modalService: ModalService, private dialogService: DialogService, private sendingPmodeService: SendingPmodeService, private receivingPmodeService: ReceivingPmodeService,
    private settingsService: SettingsService) {
        this.authenticationStore
        .changes
        .subscribe((result) => {
            this.isLoggedIn = result.loggedin;
            if (this.isLoggedIn) {
                this.settingsService.getSettings();
                this.runtimeService.getAll();
                this.sendingPmodeService.getAll();
                this.receivingPmodeService.getAll();
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
