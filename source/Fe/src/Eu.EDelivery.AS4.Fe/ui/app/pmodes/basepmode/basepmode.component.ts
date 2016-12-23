import { ModalComponent } from './../../common/modal/modal.component';
import { ModalService } from './../../common/modal/modal.service';
import { BoxComponent } from './../../common/box/box.component';
import { Subscription } from 'rxjs/Subscription';
import { Observable } from 'rxjs/Observable';
import { FormGroup, FormBuilder, FormArray, FormControl, AbstractControl } from '@angular/forms';
import { Component, ViewChildren, QueryList } from '@angular/core';

import { ReceivingPmode } from './../../api/ReceivingPmode';
import { PmodesModule } from '../pmodes.module';
import { PmodeStore } from '../pmode.store';
import { PmodeService, pmodeService } from '../pmode.service';
import { DialogService } from './../../common/dialog.service';
import { ItemType } from './../../api/ItemType';
import { RuntimeStore } from './../../settings/runtime.store';
import { SendingPmode } from './../../api/SendingPmode';
import { ReceivingProcessingMode } from './../../api/ReceivingProcessingMode';
import { getRawFormValues } from './../../common/getRawFormValues';
import { IPmode } from './../../api/Pmode.interface';

@Component({
    selector: 'as4-receiving-pmode',
    styles: [require('./basepmode.component.scss').toString()]
})
export abstract class BasePmodeComponent<T extends IPmode> {
    public form: FormGroup;
    public pmodes: string[];
    public isNewMode: boolean = false;
    public deliverSenders: Array<ItemType>;
    public newName: string;
    public actionType: string | number = 0;
    @ViewChildren(BoxComponent) boxes: QueryList<BoxComponent>;
    public get currentPmode(): T | undefined {
        return this._currentPmode;
    }
    public set currentPmode(pmode: T | undefined) {
        this._currentPmode = pmode;
        if (!!!pmode) setTimeout(() => this.form.disable());
        else setTimeout(() => this.form.enable());
    }
    protected _storeSubscription: Subscription;
    protected _currentPmodeSubscription: Subscription;
    protected _runtimeStoreSubscription: Subscription;
    protected _currentPmode: T | undefined;
    constructor(protected formBuilder: FormBuilder, protected pmodeService: PmodeService, protected pmodeStore: PmodeStore, protected dialogService: DialogService, protected runtimeStore: RuntimeStore,
        protected modalService: ModalService) {
        this.init();
    }
    public pmodeChanged(name: string) {
        let select = () => {
            this.isNewMode = false;
            let lookupPmode = this.pmodes.find(pmode => pmode === name);
            if (!!lookupPmode) this.getPmode(name); // this.pmodeService.getReceiving(name);
            else this.setPmode(undefined); // this.pmodeStore.setReceiving(undefined);
            this.form.markAsPristine();
        };
        if (this.form.dirty || this.isNewMode) {
            this.dialogService
                .confirmUnsavedChanges()
                .filter(result => result)
                .subscribe(() => {
                    if (this.isNewMode) {
                        this.pmodes = this.pmodes.filter(pmode => pmode !== this.currentPmode.name);
                        this.isNewMode = false;
                    }
                    select();
                });
            return false;
        }

        select();
        return true;
    }
    public rename() {
        this.dialogService
            .prompt('Please enter a new name')
            .filter(result => !!result)
            .subscribe(newName => {
                if (this.checkIfExists(newName)) return;
                this.form.patchValue({ [ReceivingPmode.FIELD_name]: newName });
                this.form.markAsDirty();
            });
    }
    public reset() {
        if (this.isNewMode) {
            this.isNewMode = false;
            this.pmodes = this.pmodes.filter(pmode => pmode !== this.currentPmode.name);
            this.currentPmode = undefined;
        }
        this.patchForm(this.formBuilder, this.form, this.currentPmode);
        this.form.reset(this.currentPmode);
        this.form.markAsPristine();
    }
    public delete() {
        if (!!!this.currentPmode) return;
        this.dialogService
            .deleteConfirm('pmode')
            .filter(result => result)
            .subscribe(result => this.deletePmode(this.currentPmode.name));
    }
    public add() {
        this.modalService
            .show('new-pmode')
            .filter(result => result)
            .subscribe(() => {
                if (this.checkIfExists(this.newName)) return;
                if (!!!this.newName) return;
                let newPmode: T;
                if (+this.actionType !== -1) {
                    this.getByName(this.pmodes.find(name => name === this.actionType))
                        .subscribe(existingPmode => {
                            this.currentPmode = Object.assign({}, existingPmode);
                            this.currentPmode.name = this.newName;
                            this.currentPmode.pmode.id = this.newName;
                            this.afterAdd();
                        });
                    return;
                }
                this.currentPmode = this.newPmode(this.newName);
                this.afterAdd();
            });
    }
    public save() {
        if (this.form.invalid) {
            this.dialogService.incorrectForm();
            return;
        }

        if (this.isNewMode) {
            this.createPmode(getRawFormValues(this.form))
                .subscribe(() => {
                    this.isNewMode = false;
                    this.form.markAsPristine();
                });
            return;
        }

        this.updatePmode(getRawFormValues(this.form), this.currentPmode.name)
            .subscribe(() => {
                this.isNewMode = false;
                this.form.markAsPristine();
            });
    }
    public expand() {
        let index = 0;
        this.boxes
            .filter(box => box.collapsible && ++index > 0)
            .forEach(box => box.collapsed = !box.collapsed);
    }
    ngOnDestroy() {
        this._storeSubscription.unsubscribe();
        this._currentPmodeSubscription.unsubscribe();
        this._runtimeStoreSubscription.unsubscribe();
    }
    ngAfterViewInit() {
        this.expand();
    }
    protected abstract patchForm(formBuilder: FormBuilder, form: FormGroup, pmode: T);
    protected abstract newPmode(newName: string): T;
    protected abstract getPmode(pmode: string);
    protected abstract setPmode(pmode: string | undefined);
    abstract createPmode(value: any): Observable<boolean>;
    abstract updatePmode(value: any, originalName: string): Observable<boolean>;
    abstract deletePmode(value: any);
    abstract init();
    abstract getByName(name: string): Observable<T>;
    private checkIfExists(name: string): boolean {
        let exists = this.pmodes.findIndex(pmode => pmode.toLowerCase() === name.toLowerCase()) > -1;
        if (exists) this.dialogService.message(`Pmode with name ${name} already exists`);
        return exists;
    }
    private afterAdd() {
        this.pmodes.push(this.newName);
        this.isNewMode = true;
        this.patchForm(this.formBuilder, this.form, this.currentPmode);
        this.form.markAsDirty();
    }
}
