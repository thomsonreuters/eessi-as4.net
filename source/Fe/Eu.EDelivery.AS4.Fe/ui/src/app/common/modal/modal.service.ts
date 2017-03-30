import { Observable } from 'rxjs';
import { Injectable } from '@angular/core';

import { ModalComponent } from './modal.component';

@Injectable()
export class ModalService {
    private modals: ModalComponent[] = new Array<ModalComponent>();
    public registerModal(modal: ModalComponent) {
        this.modals.push(modal);
    }
    public unregisterModal(modal: ModalComponent) {
        let index = this.modals.findIndex((src) => src === modal);
        this.modals.splice(index, 1);
    }
    public show(name: string, modal?: (dialog: ModalComponent) => void) {
        let dialog = this.modals.find((src) => src.name === name);
        this.modals.forEach((mdl) => mdl.isVisible = false);
        if (!!modal) {
            modal(dialog);
        }
        return dialog.show();
    }
}
