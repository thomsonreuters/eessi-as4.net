import { Setting } from './../../api/Setting';
import { FormGroup, FormArray, FormBuilder, AbstractControl } from '@angular/forms';
import { NgZone, Component, Input, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';

import { DialogService } from './../../common/dialog.service';
import { RuntimeService } from '../runtime.service';
import { ItemType } from './../../api/ItemType';
import { RuntimeStore } from '../runtime.store';
import { Step } from './../../api/Step';

@Component({
    selector: 'as4-step-settings',
    template: `
        <div [formGroup]="group">
            <as4-input [label]="'Decorator'">
                <select class="form-control" formControlName="decorator">
                    <option *ngFor="let step of decorators" [value]="step.technicalName">{{step.name}}</option>
                </select>
            </as4-input>
            <div>
                <p><button [disabled]="group.disabled" type="button" class="btn btn-flat" (click)="addStep()"><i class="fa fa-plus"></i></button></p>
                <table formArrayName="step" class="table table-condensed" *ngIf="group.controls.step.controls.length > 0">
                    <tbody [sortablejs]="group.controls.step" [sortablejsOptions]="{ handle: '.grippy', onEnd: itemMoved}">
                        <tr>
                            <th></th>
                            <th></th>
                            <th>Type</th>
                            <th class="col-md-1">Undecorated?</th>
                        </tr>
                        <tr *ngFor="let step of group.controls.step.controls; let i = index" [formGroupName]="i">
                            <td class="col-small"><span class="grippy"></span></td>
                            <td class="action"><button [disabled]="group.disabled" type="button" class="btn btn-flat" (click)="removeStep(i)"><i class="fa fa-trash-o"></i></button></td>
                            <td>
                                <select class="form-control" formControlName="type" (change)="stepChanged(step, selectedStep.value)" #selectedStep>    
                                    <option *ngFor="let step of steps" [value]="step.technicalName">{{step.name}}</option>
                                </select>
                                <div class="settings">
                                    <as4-runtime-settings showTitle="false" [form]="step" [types]="steps" [itemType]="step.controls.type.value"></as4-runtime-settings>
                                </div>
                            </td>
                            <td><input type="checkbox" formControlName="unDecorated"></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    `,
    styles: [
        `
            .settings {
                padding-top: 10px;
            }
        `
    ]
})
export class StepSettingsComponent implements OnDestroy {
    @Input() group: FormGroup;
    public steps: ItemType[];
    public decorators: ItemType[];
    private _runtimeStoreSubscription: Subscription;
    constructor(private formBuilder: FormBuilder, private runtimeStore: RuntimeStore, private dialogService: DialogService) {
        this._runtimeStoreSubscription = this.runtimeStore
            .changes
            .filter(result => result != null)
            .subscribe(result => {
                this.steps = result.steps;
                this.decorators = result.steps.filter(step => step.name.toLowerCase().indexOf('decorator') > 0);
            });
    }
    public itemMoved = () => {
        this.group.markAsDirty();
    }
    public addStep() {
        (<FormArray>this.group.controls['step']).push(Step.getForm(this.formBuilder, null));
        this.group.markAsDirty();
    }
    public removeStep(index: number) {
        if (!this.dialogService.confirm('Are you sure you want to delete the step ?')) return;
        (<FormArray>this.group.controls['step']).removeAt(index);
        this.group.markAsDirty();
    }
    public ngOnDestroy() {
        this._runtimeStoreSubscription.unsubscribe();
    }
    public stepChanged(formGroup: FormGroup, selectedStep: string) {
        let stepProps = this.steps.find(st => st.technicalName === selectedStep);
        formGroup.removeControl('setting');
        formGroup
            .addControl('setting', this.formBuilder.array(stepProps
                .properties
                .map(prop => Setting.getForm(this.formBuilder, {
                    key: prop.friendlyName,
                    value: ''
                }))));
    }
}
