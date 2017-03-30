import { Response } from '@angular/http';
import { Injectable, ErrorHandler } from '@angular/core';

import { SpinnerService } from '../spinner/spinner.service';

@Injectable()
export class SpinnerHideErrorHandler implements ErrorHandler {
    constructor(private _spinnerService: SpinnerService, private _errorHandler?: ErrorHandler) {

    }
    public handleError(error: Response | any): void {
        this._spinnerService.hide();
        if (!!!this._errorHandler) {
            return;
        }
        this._errorHandler.handleError(error);
    }
}

export function spinnerErrorhandlerDecoratorFactory(spinnerService: SpinnerService, errorHandler: ErrorHandler) {
    return new SpinnerHideErrorHandler(spinnerService, errorHandler);
}
