import { Subscription } from 'rxjs/Subscription';
import { NgForm, FormGroup } from '@angular/forms';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild, ViewEncapsulation, ElementRef, OnDestroy } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';

import { SubmitToolService, SubmitData, Settings } from './../submittool.service';
import { TabComponent } from './../../common/tab/tab.component';
import { ErrorResponse } from './../../api/ErrorResponse';
import { ModalService } from './../../common/modal/modal.service';
import { SignalrService } from '../signalr.service';

@Component({
    selector: 'as4-submit',
    templateUrl: 'submit.component.html',
    styleUrls: ['submit.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubmitComponent implements OnDestroy {
    public settings: Settings;
    public hasBaseDropZoneOver: boolean = false;
    public progress: number = 0;
    public logging: LogMessage[] = new Array<LogMessage>();
    public set isBusy(busy: boolean) {
        this._changeDetectorRef.detectChanges();
        this._isBusy = busy;
    }
    public get isBusy(): boolean {
        return this._isBusy;
    }
    @ViewChild('tab') public as4Tab: TabComponent;
    @ViewChild('loggingContainer') public logEl: ElementRef;
    private _isBusy: boolean = false;
    private _subscription: Subscription;
    constructor(private _submitToolService: SubmitToolService, private _changeDetectorRef: ChangeDetectorRef, private _modalService: ModalService, private _signalrService: SignalrService) {
        this.settings = this._submitToolService.settings;
        this._subscription = this._signalrService
            .onMessage
            .subscribe((result) => this.addLog(result));
    }
    public ngOnDestroy() {
        if (!!this._subscription) {
            this._subscription.unsubscribe();
        }
    }
    public submit() {
        this.isBusy = true;
        this.as4Tab.next();
        this.logging = new Array<LogMessage>();
        this.settings.payloadData.files = this.settings.uploader.getNotUploadedItems();

        if (!!!this.settings.payloadData.files || this.settings.payloadData.files.length === 0) {
            this.addLog(new LogMessage({ message: 'Uploading your request and processing it on the server.' }));
        } else {
            this.addLog(new LogMessage({ message: 'Uploading message and payload(s)', type: LogType.Upload }));
        }

        let hasError: boolean = false;
        this._submitToolService
            .upload(this.settings.payloadData)
            .finally(() => {
                this.isBusy = false;
                if (hasError) {
                    this.addLog(new LogMessage({ message: 'FAILED!', type: LogType.Error }));
                } else {
                    this.addLog(new LogMessage({ message: 'DONE - Message(s) submitted', type: LogType.Done }));
                }
            })
            .subscribe((progress) => {
                this.progress = progress;
                this._changeDetectorRef.detectChanges();
            }, (error: ErrorResponse) => {
                hasError = true;
            });
    }
    public fileOverBase(e: any): void {
        this.hasBaseDropZoneOver = e;
    }
    public totalSize(): number {
        let total = 0;
        this.settings.uploader.getNotUploadedItems().forEach((item) => total += item.file.size);
        return total;
    }
    public update(form: FormGroup) {
        form.markAsTouched();
    }
    public addLog(msg: LogMessage) {
        msg.timeStamp = new Date();
        this.logging.push(msg);
        this._changeDetectorRef.detectChanges();
        this.logEl.nativeElement.scrollTop = this.logEl.nativeElement.scrollHeight;
    }
    public logToText(): string {
        return this.logging.map((log) => `${log.timeStamp} - ${log.message}`).join('\r\n');
    }
    public open(content: string, title: string) {
        this._modalService
            .show('editor', (dlg) => {
                dlg.payload = content;
                dlg.showOk = true;
                dlg.showCancel = false;
                dlg.title = title;
            });
    }
}

// tslint:disable-next-line:max-classes-per-file
export class LogMessage {
    public timeStamp: Date;
    public message: string;
    public data: any;
    public type: LogType;
    constructor(init: Partial<LogMessage>) {
        Object.assign(this, init);
    }
}

export enum LogType {
    Info = 0,
    Error = 1,
    Upload = 2,
    Done = 3,
    Pmode = 4,
    Message = 5
}
