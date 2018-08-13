import { Router } from '@angular/router';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Component, OnInit } from '@angular/core';

import { SetupService } from './../setup.service';
import { validatePassword } from '../../common/passwordValidator';

@Component({
    selector: 'as4-setup',
    templateUrl: 'setup.component.html',
    styleUrls: ['./setup.component.scss']
})
export class SetupComponent implements OnInit {
    public form: FormGroup;
    public finished: boolean;
    constructor(private _formBuilder: FormBuilder, private _setupService: SetupService, private _router: Router) {
        this.form = this._formBuilder.group({
            adminPassword: ['', Validators.compose([Validators.required, validatePassword])],
            readonlyPassword: ['', Validators.compose([Validators.required, validatePassword])],
            jwtKey: ['', Validators.required]
        });
    }
    public generate() {
        this.form.get('jwtKey')!.setValue(this.makeid());
    }
    public send() {
        this._setupService
            .save(this.form.value)
            .subscribe((result) => this.finished = true);
    }
    public ngOnInit() {
        this._setupService
            .isSetup()
            .first()
            .subscribe((result) => {
                this.finished = result;
            });
    }
    private makeid(): string {
        let text = '';
        let possible = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';

        for (let i = 0; i < 50; i++) {
            text += possible.charAt(Math.floor(Math.random() * possible.length));
        }

        return text;
    }
}
