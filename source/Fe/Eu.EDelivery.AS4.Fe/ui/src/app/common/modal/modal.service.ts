import { Subscription } from 'rxjs/Subscription';
import { Observable } from 'rxjs/Observable';
import { Injectable, ComponentFactoryResolver, Type, ViewContainerRef, Injector, SkipSelf, Host, forwardRef, Inject } from '@angular/core';

import { ModalComponent } from './modal.component';
import { ErrorDialogComponent } from './../errorDialog/errorDialog.component';

@Injectable()
export class ModalService {
    private modals: ModalComponent[] = new Array<ModalComponent>();
    private _previousStack: ModalComponent[] = new Array<ModalComponent>();
    private _lastModal: ModalComponent | null;
    private _rootContainer: ViewContainerRef;
    constructor(private _resolver: ComponentFactoryResolver, private _injector: Injector) { }
    public registerModal(modal: ModalComponent): void {
        this.modals.push(modal);
    }
    public setRootContainerRef(container: ViewContainerRef) {
        this._rootContainer = container;
    }
    public unregisterModal(modal: ModalComponent): void {
        let index: number = this.modals.findIndex((src) => src === modal);
        this.modals.splice(index, 1);
    }
    public showComponent<TComponent>(component: Type<TComponent>, action: (dialog: TComponent) => void) {
        let factory = this._resolver.resolveComponentFactory(component);
        let cmp = this._rootContainer.createComponent(factory, this._rootContainer.length, this._injector);
        action(cmp.instance);
        cmp.onDestroy(() => cmp.destroy());
    }
    public show(name: string, modal?: (dialog: ModalComponent) => void): Observable<boolean> {
        // If there are any modals with unexpected state then stop
        if (this.modals.find((search) => search.unexpected)) {
            return Observable.of(false);
        }

        let dialog: ModalComponent | undefined = this.modals.find((src) => src.name === name);
        if (!!!dialog) {
            console.log(`No dialog found with the name ${name}`);
            return Observable.throw('Could not find modal');
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
        this.subscribeToClose(obs);
        let subscription: Subscription = obs.subscribe(() => {
            if (this._previousStack.length > 0) {
                this._lastModal = <ModalComponent>this._previousStack.pop();
                this.subscribeToClose(this._lastModal.show());
            }
            subscription.unsubscribe();
        });
        return obs;
    }
    private subscribeToClose(obs: Observable<boolean>) {
        const obsSubscription = obs.subscribe(() => {
            this._lastModal = null;
            obsSubscription.unsubscribe();
        });
    }
}
