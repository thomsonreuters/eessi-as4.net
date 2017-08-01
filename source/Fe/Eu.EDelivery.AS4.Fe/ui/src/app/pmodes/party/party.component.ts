import { Subscription } from 'rxjs/Subscription';
import { Component, Input, ContentChildren, QueryList, OnDestroy, ViewChildren } from '@angular/core';
import { FormGroup, FormArray, FormBuilder, AbstractControl } from '@angular/forms';

import { FocusDirective } from './../../common/focus.directive';
import { DialogService } from './../../common/dialog.service';
import { PartyIdForm } from './../../api/PartyIdForm';
import { Party } from './../../api/Party';
import { PartyId } from '../../api/PartyId';

@Component({
    selector: 'as4-party',
    template: `
        <div [formGroup]="group">
            <as4-input [label]="label" formArrayName="partyIds" runtimetooltip="partyids">
                <div class="item-container" *ngIf="partyIdsControl.length === 0">
                    <button as4-auth class="action add-button" type="button" [attr.disabled]="!group.disabled ? null : group.disabled" (click)="addParty()" class="btn btn-flat add-button"><i class="fa fa-plus"></i></button>
                </div>
                <div class="item-container" *ngFor="let party of partyIdsControl; let i = index" [formGroupName]="i">
                    <div class="item input"><input type="text" placeholder="id" formControlName="id" focus [focus-disabled]="!focusNew"/></div>
                    <div class="item input"><input type="text" placeholder="type" formControlName="type"/></div>
                    <div class="item actions">
                        <button as4-auth [attr.disabled]="!group.disabled ? null : group.disabled" type="button" class="remove-button btn btn-flat" (click)="removeParty(i)"><i class="fa fa-trash-o"></i></button>
                        <button as4-auth [attr.disabled]="!group.disabled ? null : group.disabled" *ngIf="i === (partyIdsControl.length-1)" type="button" [disabled]="group.disabled" (click)="addParty()" class="btn btn-flat add-button spacing"><i class="fa fa-plus"></i></button>
                    </div>
                </div>
            </as4-input>
            <as4-input label="Role" runtimetooltip="role">
                <input type="text" formControlName="role"/>
            </as4-input>
        </div>
    `,
    styles: [
        `
        @media screen and (max-width: 959px) {
            .item {
                margin-top: 9px;
            }
        }
        @media screen and (min-width: 960px) {
            .item-container {
                display: flex;
                padding-left: 0;
                margin-top: 5px;
            }
            .item {
                margin-right: 11px;
                flex: 1;
            }
            .actions {
                text-align: right;
                margin-right: 0;
                flex: 0 auto;
                min-width: 77px;
            }
        }
    `]
})
export class PartyComponent implements OnDestroy {
    @Input() public group: FormGroup;
    @Input() public label: string;
    @Input() public runtimetooltip: string;
    public focusNew: boolean = false;
    private _subscription: Subscription;
    private _previousFocusCount: number = 0;
    public get partyIdsControl(): any {
        return !!this.group && (<FormGroup>this.group.get('partyIds')).controls;
    }
    constructor(private formBuilder: FormBuilder, private dialogService: DialogService) { }
    public ngOnDestroy() {
        if (!!this._subscription) {
            this._subscription.unsubscribe();
        }
    }
    public addParty() {
        this.focusNew = true;
        let form = <FormArray>this.group.controls[Party.FIELD_partyIds];
        form.push(PartyIdForm.getForm(this.formBuilder));
        this.group.markAsDirty();
    }
    public removeParty(index: number) {
        const party = (<FormArray>this.group.controls[Party.FIELD_partyIds]).controls[index];

        if (!!party && !!!party.get(PartyId.FIELD_id)!.value && !!!party.get(PartyId.FIELD_type)!.value) {
            (<FormArray>this.group.controls[Party.FIELD_partyIds]).removeAt(index);
            return;
        }

        this.dialogService
            .deleteConfirm('Party')
            .filter((result) => result)
            .subscribe((result) => {
                (<FormArray>this.group.controls[Party.FIELD_partyIds]).removeAt(index);
                this.group.markAsDirty();
            });
    }
}
