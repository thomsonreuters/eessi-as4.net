import { Observable } from 'rxjs/Observable';
import { Component, OnInit, Input } from '@angular/core';

import { Message } from './../../api/Messages/Message';
import { Exception } from './../../api/Messages/Exception';
import { MessageService } from './../message/message.service';
import { ExceptionStore, IExceptionState } from './../exception/exception.store';

@Component({
    selector: 'as4-error-message',
    templateUrl: 'errormessage.component.html'
})
export class ErrorMessageComponent implements OnInit {
    public exceptions: Observable<IExceptionState>;
    public isLoading: boolean = false;
    @Input() public message: Message;
    @Input() public direction: number;
    constructor(private _messageStore: ExceptionStore, private _messageService: MessageService) {
        this.exceptions = _messageStore.changes;
    }
    public ngOnInit() {
        this.isLoading = true;
        this._messageService
            .getExceptions(this.direction, this.message)
            .subscribe(() => this.isLoading = false);
    }
}
