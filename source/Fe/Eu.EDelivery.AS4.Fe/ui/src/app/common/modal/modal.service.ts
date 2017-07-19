import { Subscription } from "rxjs/Subscription";
import { Observable } from "rxjs/Observable";
import { Injectable } from "@angular/core";

import { ModalComponent } from "./modal.component";

@Injectable()
export class ModalService {
    private modals: ModalComponent[] = new Array<ModalComponent>();
    private _previousStack: ModalComponent[] = new Array<ModalComponent>();
    private _lastModal: ModalComponent | null;
    public registerModal(modal: ModalComponent): void {
        this.modals.push(modal);
    }
    public unregisterModal(modal: ModalComponent): void {
        let index: number = this.modals.findIndex((src) => src === modal);
        this.modals.splice(index, 1);
    }
    public show(name: string, modal?: (dialog: ModalComponent) => void): Observable<boolean> {
        let dialog: ModalComponent | undefined = this.modals.find((src) => src.name === name);
        if (!!!dialog) {
            throw `No dialog found with the name ${name}`;
        }
        dialog.type = '';
        this.modals.forEach((mdl) => mdl.isVisible = false);
        if (!!modal) {
            modal(dialog);
        }
        if (!!this._lastModal) {
            this._previousStack.push(this._lastModal);
        }

        this._lastModal = dialog;
        let obs: Observable<boolean> = dialog.show();
        let obsSubscription: Subscription = obs.subscribe(() => {
            this._lastModal = null;
            obsSubscription.unsubscribe();
        });
        let subscription: Subscription = obs.subscribe(() => {
            if (this._previousStack.length > 0) {
                this._lastModal = <ModalComponent>this._previousStack.pop();
                this._lastModal.show();
            }
            subscription.unsubscribe();
        });
        return obs;
    }
}
