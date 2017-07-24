import { AuthHttp } from 'angular2-jwt';
import { Observer } from 'rxjs/Observer';
import { Injectable, NgZone } from '@angular/core';
import { Response } from '@angular/http';
import { Observable } from 'rxjs/Observable';
import 'rxjs/add/operator/map';
import 'rxjs/add/operator/catch';

import { TOKENSTORE } from './../authentication/token';

@Injectable()
export class SubmitToolService {
    private _baseUrl = 'api/submittool';
    constructor(private _http: AuthHttp, private _ngZone: NgZone) { }
    public simulate(submitData: SubmitData): Observable<string> {
        let data = new FormData();
        let counter = 0;
        submitData.files.map((file) => {
            data.append(`file[${counter++}]`, file.some);
        });
        data.append('pmode', submitData.pmode);
        data.append('messages', submitData.messages + '');
        return this
            ._http
            .post(`${this._baseUrl}/simulate`, data)
            .map((result) => result.text());
    }
    public upload(submitData: SubmitData): Observable<number> {
        return Observable.create((obs: Observer<number>) => {
            let data = new FormData();
            let counter = 0;
            submitData.files.map((file) => {
                data.append(`file[${counter++}]`, file.some);
            });
            data.append('pmode', submitData.pmode);
            data.append('messages', submitData.messages + '');
            let xhr = new XMLHttpRequest();

            xhr.open('POST', `${this._baseUrl}`, true);
            xhr.setRequestHeader('Authorization', 'Bearer ' + localStorage.getItem(TOKENSTORE));
            xhr.onreadystatechange = () => {
                console.log('status ' + xhr.status);
                this._ngZone.run(() => {
                    if (xhr.readyState === 4) {
                        if (xhr.status === 200) {
                            obs.complete();
                        } else if (xhr.status === 417 && !!xhr.responseText) {
                            obs.error(JSON.parse(xhr.responseText));
                        } else {
                            obs.error(JSON.parse(xhr.responseText));
                        }
                    }
                });
            };
            xhr.upload.onprogress = (event) => {
                const progress = Math.round(event.loaded / event.total * 100);
                this._ngZone.run(() => {
                    obs.next(progress);
                });
            };
            xhr.upload.onerror = (error) => {
                this._ngZone.run(() => {
                    obs.error(error);
                });
            };
            xhr.send(data);
        });
    }
}

// tslint:disable-next-line:max-classes-per-file
export class SubmitData {
    public files: any[];
    public pmode: string;
    public messages: number = 1;
}
