import { DialogService } from './../../common/dialog.service';
import { PartyId } from './../../api/PartyId';
import { Party } from './../../api/Party';
import { Component, Input } from '@angular/core';
import { FormGroup, FormArray, FormBuilder } from '@angular/forms';

@Component({
    selector: 'as4-party',
    template: `
        <div [formGroup]="group">
            <as4-input [label]="label" formArrayName="partyIds">
                <div class="item-container" *ngIf="group.controls.partyIds.controls.length === 0">
                    <button class="action add-button" type="button" [disabled]="group.disabled" (click)="addParty()" class="btn btn-flat add-button"><i class="fa fa-plus"></i></button>
                </div>
                <div class="item-container" *ngFor="let party of group.controls.partyIds.controls; let i = index" [formGroupName]="i">
                    <div class="item input"><input type="text" placeholder="id" formControlName="id"/></div>
                    <div class="item input"><input type="text" placeholder="type" formControlName="type"/></div>
                    <div class="item actions">
                        <button [disabled]="group.disabled" type="button" class="remove-button btn btn-flat" (click)="removeParty(i)"><i class="fa fa-trash-o"></i></button>
                        <button [disabled]="group.disabled" *ngIf="i === (group.controls.partyIds.controls.length-1)" type="button" [disabled]="group.disabled" (click)="addParty()" class="btn btn-flat add-button spacing"><i class="fa fa-plus"></i></button>
                    </div>
                </div>
            </as4-input>
            <as4-input label="Role">
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
export class PartyComponent {
    @Input() group: FormGroup;
    @Input() label: string;
    constructor(private formBuilder: FormBuilder, private dialogService: DialogService) {
    }
    public addParty() {
        let form = <FormArray>this.group.controls[Party.FIELD_partyIds];
        form.push(PartyId.getForm(this.formBuilder, null));
        this.group.markAsDirty();
    }
    public removeParty(index: number) {
        this.dialogService
            .deleteConfirm('Party')
            .filter(result => result)
            .subscribe(result => {
                (<FormArray>this.group.controls[Party.FIELD_partyIds]).removeAt(index);
                this.group.markAsDirty();
            });
    }
}
