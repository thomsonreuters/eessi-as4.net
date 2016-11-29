import { ErrorHandler, Injectable } from '@angular/core';

@Injectable()
export class DialogErrorHandler implements ErrorHandler {
    handleError(error: any): void {
        alert(error);
    }
}

export var LOGGING_ERROR_HANDLER_PROVIDER = [
    {
        provide: ErrorHandler,
        useClass: DialogErrorHandler
    }
];
