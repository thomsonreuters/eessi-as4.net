import { FormGroup, FormArray, FormBuilder, AbstractControl } from '@angular/forms';
import { Component, Input, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';

import { RuntimeService, ItemType } from './runtime.service';
import { RuntimeStore } from './runtime.store';
import { Step } from './../api/Step';

@Component({
    selector: 'as4-step-settings',
    template: `
        <div [formGroup]="group">
            <as4-input [label]="'Decorator'">
                <select class="form-control" formControlName="decorator" (change)="decoratorChanged($event.target.value)">
                    <option *ngFor="let step of decorators" [value]="step.name">{{step.name}}</option>
                </select>
            </as4-input>
            <p><button type="button" class="btn btn-flat" (click)="addStep()">Add</button></p>
            <table formArrayName="step" class="table table-condensed">
                <tbody>
                    <tr>
                        <th class="col-md-10">Name</th>
                        <th class="col-md-2">Is undecorated?</th>
                        <th></th>
                    </tr>
                    <tr *ngFor="let step of group.controls.step.controls; let i = index" [formGroupName]="i">
                        <td>
                            <select class="form-control" formControlName="type">
                                <option *ngFor="let step of steps" [value]="step.name">{{step.name}}</option>
                            </select>
                        </td>
                        <td><input type="checkbox" formControlName="unDecorated"></td>
                        <td><button type="button" class="btn btn-flat" (click)="removeStep(i)">Remove</button></td>                        
                    </tr>
                </tbody>
            </table>
        </div>
    `
})
export class StepSettingsComponent implements OnDestroy {
    @Input() group: FormGroup;
    public steps: ItemType[];
    public decorators: ItemType[];
    private _runtimeStoreSubscription: Subscription;
    constructor(private formBuilder: FormBuilder, private runtimeStore: RuntimeStore) {
        this._runtimeStoreSubscription = this.runtimeStore
            .changes
            .filter(result => result != null)
            .subscribe(result => {
                this.steps = result.steps;
                this.decorators = result.steps.filter(step => step.name.toLowerCase().indexOf('decorator') > 0);
            });
    }
    public addStep() {
        (<FormArray>this.group.controls['step']).push(Step.getForm(this.formBuilder, null));
        this.group.markAsDirty();
    }
    public removeStep(index: number) {
        (<FormArray>this.group.controls['step']).removeAt(index);
        this.group.markAsDirty();
    }
    public decoratorChanged(value: string) {
        var step = this.runtimeStore.getState().steps.filter(step => step.name == value);
        console.log(step);
    }
    public ngOnDestroy() {
        this._runtimeStoreSubscription.unsubscribe();
    }
}