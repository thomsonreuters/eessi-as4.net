import { Response } from '@angular/http';
import { ErrorHandler, Injectable } from '@angular/core';

import { ErrorResponse } from '../api';
import { DialogService } from './dialog.service';
import { SpinnerService } from './spinner/spinner.service';

@Injectable()
export class DialogErrorHandler implements ErrorHandler {
    constructor(private _dialogService: DialogService, private _spinnerService: SpinnerService) { }
    public handleError(error: Response | any): void {
        this._spinnerService.hide();
        // tslint:disable-next-line:max-line-length
        this._dialogService.error(`An unexpected error has occured. This means that a stable application can't be guaranteed. A refresh is required!`, error, true);
        console.error(error);
    }
}

export function errorHandlerFactory(dialogService: DialogService, spinnerService: SpinnerService) {
    return new DialogErrorHandler(dialogService, spinnerService);
}
