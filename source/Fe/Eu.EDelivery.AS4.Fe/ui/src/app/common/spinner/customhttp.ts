import { Observable } from 'rxjs';
import { Http, XHRBackend, Request, RequestOptionsArgs, RequestOptions, Response } from '@angular/http';

import { SpinnerService } from './spinner.service';
import { DialogService } from './../dialog.service';

export class CustomHttp extends Http {
    // tslint:disable-next-line:max-line-length
    constructor(backend: XHRBackend, options: RequestOptions, private spinnerService: SpinnerService, private _dialogService: DialogService) {
        super(backend, options);
    }
    public request(url: string | Request, options?: RequestOptionsArgs): Observable<Response> {
        this.spinnerService.show();
        let result = super
            .request(url, options)
            .do(() => this.spinnerService.hide())
            .catch((error) => {
                // tslint:disable-next-line:max-line-length
                this._dialogService.error(`An error occured while communicating with the API. Please verify that you can reach the API. Please reload the application to try again.`, error, true);
                return Observable.throw(error);
            });
        return result;
    }
}