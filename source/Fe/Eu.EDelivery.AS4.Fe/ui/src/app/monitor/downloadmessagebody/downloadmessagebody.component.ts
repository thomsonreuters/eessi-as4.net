import { ExceptionService } from './../exception/exception.service';
import { Component, Input } from '@angular/core';

import { MessageService } from './../message/message.service';
import * as fileSaver from 'file-saver';

@Component({
    selector: 'downloadmessagebody',
    template: `
        <a (click)="download()" class="clickable">Download&nbsp;<i class="fa fa-download clickable"></i></a>
    `
})
export class DownloadMessageBodyComponent {
    @Input() public direction: number;
    @Input() public messageId: string;
    @Input() public type: string = 'message';
    constructor(private _messageService: MessageService, private _exceptionService: ExceptionService) {
    }
    public download() {
        let service;
        if (this.type === 'exception') {
            service = this._exceptionService.getExceptionBody(this.direction, this.messageId);
        } else {
            service = this._messageService.getMessageBody(this.direction, this.messageId);
        }

        service.subscribe((result) => {
            let blob: Blob = new Blob([result], { type: 'application/text' });
            fileSaver.saveAs(blob, `${this.messageId}.txt`);
        });
    }
}
