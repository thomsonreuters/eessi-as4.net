import { CanComponentDeactivate } from './../../common/candeactivate.guard';
import { FormBuilder, FormGroup, FormArray, FormControl, AbstractControl, Validators } from '@angular/forms';
import { Observable } from 'rxjs/Observable';
import { Component, Output, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';

import { DialogService } from './../../common/dialog.service';
import { AuthorizationMapService } from './authorizationmapservice';
import { PullRequestAuthorizationEntry } from './../../api/PullRequestAuthorizationEntry';
import { thumbPrintValidation } from '../../common/thumbprintInput/validator';

@Component({
    selector: 'as4-authorizationmap',
    templateUrl: 'authorizationmap.component.html',
    styles: [`
        .center {
            text-align: center !important;
        }
    `]
})
export class AuthorizationMapComponent implements CanComponentDeactivate {
    public form: FormArray;
    @Output() public isSaveEnabled: Observable<boolean>;
    constructor(private _authorizationMapService: AuthorizationMapService, private _dialogService: DialogService, private _formBuilder: FormBuilder) {
        this._authorizationMapService
            .get()
            .subscribe((result) => {
                result.forEach((item) => {
                    this.add(item);
                });
            });
        this.form = this._formBuilder.array([]);
        this.isSaveEnabled = this.form.valueChanges.map((state) => this.form.dirty && this.form.valid);
    }
    public canDeactivate(): boolean {
        return this.form.dirty;
    }

    public add(entry: PullRequestAuthorizationEntry | null = null) {
        this.form.push(this._formBuilder.group({
            mpc: [!!entry ? entry.mpc : null, Validators.required],
            certificateThumbprint: [!!entry ? entry.certificateThumbprint : null, Validators.compose([thumbPrintValidation, Validators.required])],
            allowed: [!!entry ? entry.allowed : false]
        }));
    }
    public remove(item: AbstractControl) {
        if (!!item.get('certificateThumbprint')!.value || !!item.get('mpc')!.value) {
            this._dialogService
                .confirm('Are you sure you want to delete the authorization map?', 'Delete')
                .filter((result) => result)
                .subscribe(() => this.removeFromArray(item));
        } else {
            this.removeFromArray(item);
        }
    }
    public save() {
        this._authorizationMapService
            .post(this.form.value)
            .subscribe(() => {
                this.form.markAsPristine();
                this.form.updateValueAndValidity();
            });
    }
    private removeFromArray(item: AbstractControl) {
        let index = this.form.controls.indexOf(item);
        this.form.removeAt(index);
        this.form.markAsDirty();
        this.form.updateValueAndValidity();
    }
}
