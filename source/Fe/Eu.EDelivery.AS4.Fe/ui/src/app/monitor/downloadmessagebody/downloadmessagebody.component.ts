import { ExceptionService } from './../exception/exception.service';
import { Component, Input } from '@angular/core';

import { MessageService } from './../message/message.service';
import * as fileSaver from 'file-saver';

@Component({
    selector: 'as4-downloadmessagebody',
    template: `
        
<i class="fa fa-download clickable" (click)="download()" as4-tooltip="{{tooltip}}"></i>
    `
})
export class DownloadMessageBodyComponent {
    @Input() public direction: number;
    @Input() public id: string;
    @Input() public type: string = 'message';
    @Input() public tooltip: string = 'Download message body';
    constructor(private _messageService: MessageService, private _exceptionService: ExceptionService) { }
    public download() {
        let service;
        if (this.type === 'exception') {                service = this._exceptionService.getExceptionBody(this.direction, this.id);
        } else {
            service = this._messageService.getMessageBody(this.direction, this.id);
        }

        service.subscribe((result) => {
            let blob: Blob = new Blob([result], { type: 'application/xml' });
            fileSaver.saveAs(blob, `${this.id}.xml`);
        });
    }
}
