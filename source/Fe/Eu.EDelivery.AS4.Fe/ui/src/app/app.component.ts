import { Component, ViewEncapsulation, ViewChild, ElementRef, ViewContainerRef, AfterViewInit } from '@angular/core';

import { AppState } from './app.service';
import { RuntimeService } from './settings/runtime.service';
import { AuthenticationStore } from './authentication/authentication.store';
import { SendingPmodeService } from './pmodes/sendingpmode.service';
import { ReceivingPmodeService } from './pmodes/receivingpmode.service';
import { SettingsService } from './settings/settings.service';
import { DialogService } from './common/dialog.service';
import { ModalService } from './common/modal/modal.service';

import 'jquery';
import 'bootstrap/dist/js/bootstrap.js';
import '../theme/js/app.js';

@Component({
    selector: 'as4-app',
    encapsulation: ViewEncapsulation.None,
    styleUrls: ['./app.component.scss'],
    template: `    
        <as4-modal name="default"></as4-modal>
        <as4-modal name="prompt" #promptDialog (shown)="input.focus(); promptDialog.result = ''">
            <input type="text" class="form-control" #input [value]="promptDialog.result" (keyup)="promptDialog.result = $event.target.value"/>             
        </as4-modal>
        <as4-modal name="error" showDefaultButtons="false" #errorDialog>
            <div *ngIf="isShowDetails && !!errorDialog.payload" [class.stack-trace]="isShowDetails">
                <h4>Stack trace</h4>
                <i class="fa fa-clipboard clickable" [ngxClipboard]="payload" (cbOnSuccess)="tooltip.show()" as4-tooltip="Copied to clipboard" as4-tooltip-manual="true" #tooltip="as4-tooltip"></i>
                <p #payload>{{errorDialog.payload}}</p>
            </div>
            <div buttons>
                <button type="button" class="btn" *ngIf="!!errorDialog.payload" (click)="isShowDetails = !!!isShowDetails ? true : isShowDetails">DETAILS</button>
                <button type="button" class="btn" *ngIf="showOk" (click)="errorDialog.ok()" focus>OK</button>
            </div>
        </as4-modal>
        <as4-spinner></as4-spinner>
        <router-outlet></router-outlet>
  `
})
export class AppComponent implements AfterViewInit {
    public isLoggedIn: boolean;
    public isShowDetails: boolean = false;
    public showOk: boolean = true;
    @ViewChild('modal') public modal: ElementRef;
    // tslint:disable-next-line:max-line-length
    constructor(private appState: AppState, private authenticationStore: AuthenticationStore, private runtimeService: RuntimeService, private modalService: ModalService, private dialogService: DialogService, private sendingPmodeService: SendingPmodeService, private receivingPmodeService: ReceivingPmodeService,
        private settingsService: SettingsService, private _viewContainer: ViewContainerRef) {
        this.modalService.setRootContainerRef(this._viewContainer);

        this.authenticationStore
            .changes
            .subscribe((result) => {
                this.isLoggedIn = result.loggedin;
                if (this.isLoggedIn) {
                    this.settingsService.getSettings();
                    this.sendingPmodeService.getAll();
                    this.receivingPmodeService.getAll();
                }
            });
    }
    public ngAfterViewInit() {
        if (!this.runtimeService.isLoaded) {
            this.dialogService.error('Error connecting to the API. Please verify that you can reach it! Reload the browser to try again.', 'Error', true);
            return;
        }
    }
}
