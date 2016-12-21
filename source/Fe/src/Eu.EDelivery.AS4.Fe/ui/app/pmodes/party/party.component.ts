import { DialogService } from './../../common/dialog.service';
import { PartyId } from './../../api/PartyId';
import { Party } from './../../api/Party';
import { Component, Input } from '@angular/core';
import { FormGroup, FormArray, FormBuilder } from '@angular/forms';

@Component({
    selector: 'as4-party',
    template: `
        <div [formGroup]="group">
            <as4-input [label]="label" labelSize="2" controlSize="4">
                <table class="table table-condensed" formArrayName="partyIds">                   
                    <tr *ngIf="group.controls.partyIds.controls.length === 0">
                        <td class="party-actions padding-fix">
                            <button class="add-button" type="button" [disabled]="group.disabled" (click)="addParty()" class="btn btn-flat add-button"><i class="fa fa-plus"></i></button>
                        </td>
                        <td></td>
                        <td></td>
                    </tr>
                    <tr *ngFor="let party of group.controls.partyIds.controls; let i = index" [formGroupName]="i">                       
                        <td><input type="text" class="form-control" formControlName="id"/></td>
                        <td><input type="text" class="form-control" formControlName="type"/></td>
                        <td class="party-actions">
                            <button [disabled]="group.disabled" type="button" class="remove-button btn btn-flat" (click)="removeParty(i)"><i class="fa fa-trash-o"></i></button>
                            <button [disabled]="group.disabled" *ngIf="i === (group.controls.partyIds.controls.length-1)" type="button" [disabled]="group.disabled" (click)="addParty()" class="btn btn-flat add-button spacing"><i class="fa fa-plus"></i></button>
                        </td>
                    </tr>
                </table>
            </as4-input>
            <as4-input label="Role" labelSize="2" controlSize="4">
                <input type="text" class="form-control" formControlName="role"/>
            </as4-input>
        </div>
    `,
    styles: [
        `
        .party-actions {
            width: 2à%;
        }

        table {
            margin-bottom: 0;
        }

        tr > td {
            padding: 0;
            padding-right: 5px;
            border-top: 0;
        }
    
        tr:not(:last-child) > td {
            padding-bottom: 5px;
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
