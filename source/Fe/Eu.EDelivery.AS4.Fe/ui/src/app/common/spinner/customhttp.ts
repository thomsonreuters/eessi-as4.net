import { Observable } from 'rxjs';
import { Http, XHRBackend, Request, RequestOptionsArgs, RequestOptions, Response } from '@angular/http';

import { SpinnerService } from './spinner.service';

export class CustomHttp extends Http {
    constructor(backend: XHRBackend, options: RequestOptions, private spinnerService: SpinnerService) {
        super(backend, options);
    }
    public request(url: string | Request, options?: RequestOptionsArgs): Observable<Response> {
        this.spinnerService.show();
        let result = super
            .request(url, options)
            .do(() => this.spinnerService.hide());
        return result;
    }
}