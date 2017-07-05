import { RouterService } from './../../common/router.service';
import { ActivatedRoute, Router } from '@angular/router';
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
                <select class="FormArray-control select-pmode" as4-no-auth (change)="pmodeChanged($event.target.value); pmodeSelect.value = currentPmode && currentPmode.name" #pmodeSelect>
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
    public isNewMode: boolean = false;
    public pmodes: string[];
    public form: FormGroup;
    public currentPmode: IPmode;
    public actionType: string;
    public newName: string;
    private subscriptions: Subscription[] = new Array<Subscription>();
    constructor(private _dialogService: DialogService, @Inject(PMODECRUD_SERVICE) private _crudService: ICrudPmodeService, private _modalService: ModalService, private _activatedRoute: ActivatedRoute,
        private _router: Router, private _routerService: RouterService) {
        this.form = this._crudService.getForm(null).build();
    }
    public ngOnInit() {
        if (!!!this._activatedRoute.snapshot.params['pmode']) {
            this._crudService.get(null);
        }

        this.subscriptions
            .push(this._crudService
                .obsGetAll()
                .subscribe((result) => {
                    this.pmodes = result;
                    let pmodeQueryParam = this._activatedRoute.snapshot.params['pmode'];
                    if (!!!pmodeQueryParam) {
                        return;
                    }
                    if (result.length === 0) {
                        return;
                    }
                    // Validate that the requested pmode exists
                    let exists = result.find((search) => search === pmodeQueryParam);
                    if (!!!exists) {
                        this._dialogService.message(`Pmode ${pmodeQueryParam} doesn't exist`);
                        return;
                    }
                    this._crudService.get(pmodeQueryParam);
                }));
        this.subscriptions
            .push(this._crudService.obsGet().subscribe((result) => {
                this.currentPmode = result;
                this.form = this._crudService.getForm(result).build();
                this.form.markAsPristine();
                if (!!!result) {
                    return;
                }
                let compareTo = this._activatedRoute.snapshot.queryParams['compareto'];
                if (!!result && !!compareTo && compareTo !== result.hash) {
                    this._dialogService.error(`Pmode used in the message doesn't match anymore.`);
                }
                this._routerService.setCurrentValue(this._activatedRoute, result.name);
            }));
    }
    public ngOnDestroy() {
        this.subscriptions.forEach((subs) => subs.unsubscribe());
    }
    public pmodeChanged(name: string) {
        let select = () => {
            this.isNewMode = false;
            let lookupPmode = this.pmodes.find((pmode) => pmode === name);
            this._crudService.get(name);
        };
        if (this.form.dirty || this.isNewMode) {
            this._dialogService
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
        this.form = this._crudService.getForm(this.currentPmode).build();
        this.form.markAsPristine();
    }
    public delete() {
        if (!!!this.currentPmode) {
            return;
        }
        this._dialogService
            .deleteConfirm('pmode')
            .filter((result) => result)
            .subscribe((result) => this._crudService.delete(this.currentPmode.name));
    }
    public add() {
        this._modalService
            .show('new-pmode')
            .filter((result) => result)
            .subscribe(() => {
                if (this.checkIfExists(this.newName)) {
                    return;
                }
                if (!!!this.newName) {
                    return;
                }
                if (+this.actionType !== -1) {
                    this._crudService
                        .getByName(this.pmodes.find((name) => name === this.actionType))
                        .subscribe((existingPmode) => {
                            this.currentPmode = Object.assign({}, existingPmode);
                            this.currentPmode.name = this.newName;
                            this.currentPmode.pmode.id = this.newName;
                            this.afterAdd();
                        });
                    return;
                }
                this.currentPmode = this._crudService.getNew(this.newName);
                this.afterAdd();
            });
    }
    public rename() {
        this._dialogService
            .prompt('Please enter a new name')
            .filter((result) => !!result)
            .subscribe((newName) => {
                if (this.checkIfExists(newName)) {
                    return;
                }
                this._crudService.patchName(this.form, newName);
                this.form.markAsDirty();
            });
    }
    public save() {
        if (this.form.invalid) {
            this._dialogService.incorrectForm();
            return;
        }

        if (this.isNewMode) {
            this._crudService
                .create(getRawFormValues(this.form))
                .subscribe(() => {
                    this.isNewMode = false;
                    this.form.markAsPristine();
                });
            return;
        }

        this._crudService
            .update(getRawFormValues(this.form), this.currentPmode.name)
            .subscribe(() => {
                this.isNewMode = false;
                this.form.markAsPristine();
            });
    }
    private checkIfExists(name: string): boolean {
        let exists = this.pmodes.findIndex((pmode) => pmode.toLowerCase() === name.toLowerCase()) > -1;
        if (exists) {
            this._dialogService.message(`Pmode with name ${name} already exists`);
        }
        return exists;
    }
    private afterAdd() {
        this.pmodes.push(this.newName);
        this.isNewMode = true;
        this.form = this._crudService.getForm(this.currentPmode).build();
        this.form.markAsDirty();
    }
}
