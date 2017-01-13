import { Response } from '@angular/http';

import { ModalComponent } from './modal/modal.component';
import { ModalService } from './modal/modal.service';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs/Observable';
import { Subject } from 'rxjs/Subject';

@Injectable()
export class DialogService {
    constructor(private modalService: ModalService) {

    }
    public prompt(message: string, title?: string): Observable<string> {
        let obs = new Subject<string>();
        let dialog: ModalComponent;
        this.modalService
            .show('prompt', (dlg) => {
                dlg.message = message;
                dlg.title = title;
                dialog = dlg;
            })
            .filter(result => result)
            .subscribe(() => {
                obs.next(dialog.result);
                obs.complete();
            });
        return obs.asObservable();
    }
    public confirm(message: string, title?: string): Observable<boolean> {
        return Observable
            .create(observer => {
                this.modalService
                    .show('default', (dlg) => {
                        dlg.message = message;
                        dlg.title = title;
                    })
                    .subscribe(result => {
                        observer.next(result);
                        observer.complete();
                    });
            });
    }
    public confirmUnsavedChanges(): Observable<boolean> {
        return this.confirm('There are unsaved changes, are you sure you want to continue?', 'Unsaved changes');
    }
    public message(message: string) {
        this.modalService
            .show('default', (dlg) => {
                dlg.message = message;
                dlg.showCancel = false;
            });
    }
    public error(message: string, stackTrace: string | Response) {
        this.modalService
            .show('error', (dlg) => {
                dlg.type = 'modal-danger';
                dlg.message = message;
                dlg.showCancel = false;
                dlg.buttonOk = 'Ok';
                dlg.title = 'Error';
                if (stackTrace instanceof Response)
                    try {
                        dlg.payload = stackTrace.json().Exception;
                    }
                    catch (e) {

                    }
            });
    }
    public incorrectForm() {
        this.modalService
            .show('default', (dlg) => {
                dlg.message = 'Input is invalid, please correct the invalid fields';
                dlg.showCancel = false;
            });
    }
    public deleteConfirm(type: string): Observable<boolean> {
        return Observable
            .create(observer => {
                this.modalService
                    .show('default', (dlg) => {
                        dlg.title = `Delete ${type}`;
                        dlg.message = `Are you sure you want to delete the ${type} ?`;
                        dlg.buttonCancel = 'No';
                        dlg.buttonOk = 'Yes';
                    })
                    .subscribe(result => {
                        observer.next(result);
                        observer.complete();
                    });
            });
    }
}
