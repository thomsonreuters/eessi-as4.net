import { NgForm, FormGroup } from '@angular/forms';
import { Component, OnInit, ChangeDetectionStrategy, ChangeDetectorRef, ViewChild, ViewEncapsulation } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';

import { SubmitToolService, SubmitData } from './../submittool.service';
import { TabComponent } from './../../common/tab/tab.component';
import { ErrorResponse } from './../../api/ErrorResponse';

@Component({
    selector: 'as4-submit',
    templateUrl: 'submit.component.html',
    styleUrls: ['submit.component.scss'],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SubmitComponent {
    public uploader: FileUploader = new FileUploader({ url: '' });
    public hasBaseDropZoneOver: boolean = false;
    public progress: number = 0;
    public payloadData: SubmitData = new SubmitData();
    public logging: LogMessage[] = new Array<LogMessage>();
    public set isBusy(busy: boolean) {
        this._changeDetectorRef.detectChanges();
        this._isBusy = busy;
    }
    public get isBusy(): boolean {
        return this._isBusy;
    }
    @ViewChild('tab') public as4Tab: TabComponent;
    private _isBusy: boolean = false;
    constructor(private _submitToolService: SubmitToolService, private _changeDetectorRef: ChangeDetectorRef) { }
    public submit() {
        this.isBusy = true;
        this.as4Tab.next();
        this.logging = new Array<LogMessage>();
        this.payloadData.files = this.uploader.getNotUploadedItems();
        if (!!!this.payloadData.files || this.payloadData.files.length === 0) {
            this.addLog('Uploading your request and processing it on the server.');
        } else {
            this.addLog('Uploading message and payload(s)', LogType.Upload);
        }
        let hasError: boolean = false;
        this._submitToolService
            .upload(this.payloadData)
            .finally(() => {
                this.isBusy = false;
                if (hasError) {
                    this.addLog('FAILED!', LogType.Error);
                } else {
                    this.addLog('DONE - Message(s) submitted', LogType.Done);
                }
            })
            .subscribe((progress) => {
                this.progress = progress;
                this._changeDetectorRef.detectChanges();
                if (progress === 100 && this.payloadData.files.length > 0) {
                    this.addLog('Upload finished, now processing your request on the server');
                }
            }, (error: ErrorResponse) => {
                hasError = true;
                this.addLog(error.Message, LogType.Error);
            });
    }
    public fileOverBase(e: any): void {
        this.hasBaseDropZoneOver = e;
    }
    public totalSize(): number {
        let total = 0;
        this.uploader.getNotUploadedItems().forEach((item) => total += item.file.size);
        return total;
    }
    public update(form: FormGroup) {
        form.markAsTouched();
    }
    public addLog(msg: string, type: LogType = LogType.Info) {
        this.logging.push(new LogMessage({
            timeStamp: new Date(),
            message: msg,
            type
        }));
        this._changeDetectorRef.detectChanges();
    }
    public logToText(): string {
        return this.logging.map((log) => `${log.timeStamp} - ${log.message}`).join('\r\n');
    }
}

// tslint:disable-next-line:max-classes-per-file
export class LogMessage {
    public timeStamp: Date;
    public message: string;
    public type: LogType;
    constructor(init: Partial<LogMessage>) {
        Object.assign(this, init);
    }
}

export enum LogType {
    Info = 0,
    Error = 1,
    Upload = 2,
    Done = 3
}
