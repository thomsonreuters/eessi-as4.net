import { SpinnerService } from './spinner/spinner.service';
import { Response } from '@angular/http';
import { DialogService } from './dialog.service';
import { ErrorHandler, Injectable } from '@angular/core';

@Injectable()
export class DialogErrorHandler implements ErrorHandler {
    constructor(private _dialogService: DialogService, private _spinnerService: SpinnerService) {

    }
    public handleError(error: Response | any): void {
        this._spinnerService.hide();
        this._dialogService.error(error, error);
    }
}

export function errorHandlerFactory(dialogService: DialogService, spinnerService: SpinnerService) {
    return new DialogErrorHandler(dialogService, spinnerService);
}