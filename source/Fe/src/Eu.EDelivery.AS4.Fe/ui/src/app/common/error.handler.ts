import { Response } from '@angular/http';
import { DialogService } from './dialog.service';
import { ErrorHandler, Injectable } from '@angular/core';

@Injectable()
export class DialogErrorHandler implements ErrorHandler {
    constructor(private _dialogService: DialogService) {

    }
    handleError(error: Response | any): void {
        this._dialogService.error(error, error);
    }
}

export var LOGGING_ERROR_HANDLER_PROVIDER = [
    {
        provide: ErrorHandler,
        useClass: DialogErrorHandler
    }
];
