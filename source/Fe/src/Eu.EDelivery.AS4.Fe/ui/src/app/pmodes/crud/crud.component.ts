import { FormGroup, FormBuilder } from '@angular/forms';
import { Component, OnInit, OpaqueToken, Inject, Output, OnDestroy } from '@angular/core';

import { DialogService } from './../../common/dialog.service';
import { IPmode } from './../../api/Pmode.interface';
import { ModalService } from './../../common/modal/modal.service';
import { getRawFormValues } from './../../common/getRawFormValues';
import { Subscription } from 'rxjs/Subscription';
import { ICrudPmodeService } from '../crudpmode.service.interface';

export const PMODECRUD_SERVICE = new OpaqueToken('pmodecrudservice');

@Component({
    selector: 'as4-pmode',
    template: `
        <as4-modal name="new-pmode" title="Create a new pmode" (shown)="actionType = pmodes[0]; newName = ''; nameInput.focus()">
            <form class="form-horizontal">
                <div class="form-group">
                    <label class="col-xs-2">New name</label>
                    <div class="col-xs-10">
                        <input type="text" class="form-control" #nameInput (keyup)="newName = $event.target.value" [value]="newName" />
                    </div>
                </div>
                <div class="form-group">
                    <label class="col-xs-2">Use</label>
                    <div class="col-xs-10">
                        <select class="form-control" (change)="actionType = $event.target.value" #select>
                            <option *ngFor="let setting of pmodes" [selected]="actionType === setting" [ngValue]="setting">{{setting}}</option>
                            <option value="-1" [selected]="actionType === '-1'">Custom</option>
                        </select>
                    </div>
                </div>
            </form>
        </as4-modal>
        <as4-input label="Name" runtimeTooltip="receivingprocessingmode.id">
            <as4-columns noMargin="true">
                <select class="FormArray-control select-pmode" (change)="pmodeChanged($event.target.value); pmodeSelect.value = currentPmode && currentPmode.name" #pmodeSelect>
                    <option value="undefined">Select an option</option>
                    <option *ngFor="let pmode of pmodes" [selected]="pmode === (currentPmode && currentPmode.name)">{{currentPmode && currentPmode.name === pmode && !!form && !!form.controls ? form.controls.name.value : pmode}}</option>
                </select>
                <div crud-buttons [form]="form" (add)="add()" (rename)="rename()" (reset)="reset()" (delete)="delete()" (save)="save()" [current]="currentPmode"
                    [isNewMode]="isNewMode"></div>
            </as4-columns>
        </as4-input>
        <ng-content></ng-content>
    `
})
export class CrudComponent implements OnInit, OnDestroy {
    public isNewMode: boolean;
    public pmodes: string[];
    public form: FormGroup;
    public currentPmode: IPmode;
    public actionType: string;
    public newName: string;
    private subscriptions: Subscription[] = new Array<Subscription>();
    constructor(private dialogService: DialogService, @Inject(PMODECRUD_SERVICE) private crudService: ICrudPmodeService, private modalService: ModalService) {
        this.form = this.crudService.getForm(null);
    }
    public ngOnInit() {
        this.subscriptions.push(this.crudService.obsGetAll().subscribe((result) => this.pmodes = result));
        this.subscriptions.push(this.crudService.obsGet().subscribe((result) => {
            this.currentPmode = result;
            this.crudService.patchForm(this.form, result);
            this.form.markAsPristine();
        }));
    }
    public ngOnDestroy() {
        this.subscriptions.forEach((subs) => subs.unsubscribe());
    }
    public pmodeChanged(name: string) {
        let select = () => {
            this.isNewMode = false;
            let lookupPmode = this.pmodes.find((pmode) => pmode === name);
            this.crudService.get(name);
        };
        if (this.form.dirty || this.isNewMode) {
            this.dialogService
                .confirmUnsavedChanges()
                .filter((result) => result)
                .subscribe(() => {
                    if (this.isNewMode) {
                        this.pmodes = this.pmodes.filter((pmode) => pmode !== this.currentPmode.name);
                        this.isNewMode = false;
                    }
                    select();
                });
            return false;
        }

        select();
        return true;
    }
    public reset() {
        if (this.isNewMode) {
            this.isNewMode = false;
            this.pmodes = this.pmodes.filter((pmode) => pmode !== this.currentPmode.name);
            this.currentPmode = undefined;
        }
        this.crudService.patchForm(this.form, this.currentPmode);
        this.form.markAsPristine();
    }
    public delete() {
        if (!!!this.currentPmode) {
            return;
        }
        this.dialogService
            .deleteConfirm('pmode')
            .filter((result) => result)
            .subscribe((result) => this.crudService.delete(this.currentPmode.name));
    }
    public add() {
        this.modalService
            .show('new-pmode')
            .filter((result) => result)
            .subscribe(() => {
                if (this.checkIfExists(this.newName)) {
                    return;
                }
                if (!!!this.newName) {
                    return;
                }
                let newPmode: IPmode;
                if (+this.actionType !== -1) {
                    this.crudService
                        .getByName(this.pmodes.find((name) => name === this.actionType))
                        .subscribe((existingPmode) => {
                            this.currentPmode = Object.assign({}, existingPmode);
                            this.currentPmode.name = this.newName;
                            this.currentPmode.pmode.id = this.newName;
                            this.afterAdd();
                        });
                    return;
                }
                this.currentPmode = this.crudService.getNew(this.newName);
                this.afterAdd();
            });
    }
    public rename() {
        this.dialogService
            .prompt('Please enter a new name')
            .filter((result) => !!result)
            .subscribe((newName) => {
                if (this.checkIfExists(newName)) {
                    return;
                }
                this.crudService.patchName(this.form, newName);
                this.form.markAsDirty();
            });
    }
    public save() {
        if (this.form.invalid) {
            this.dialogService.incorrectForm();
            return;
        }

        if (this.isNewMode) {
            this.crudService
                .create(getRawFormValues(this.form))
                .subscribe(() => {
                    this.isNewMode = false;
                    this.form.markAsPristine();
                });
            return;
        }

        this.crudService
            .update(getRawFormValues(this.form), this.currentPmode.name)
            .subscribe(() => {
                this.isNewMode = false;
                this.form.markAsPristine();
            });
    }
    private checkIfExists(name: string): boolean {
        let exists = this.pmodes.findIndex((pmode) => pmode.toLowerCase() === name.toLowerCase()) > -1;
        if (exists) {
            this.dialogService.message(`Pmode with name ${name} already exists`);
        }
        return exists;
    }
    private afterAdd() {
        this.pmodes.push(this.newName);
        this.isNewMode = true;
        this.crudService.patchForm(this.form, this.currentPmode);
        this.form.markAsDirty();
    }
}
