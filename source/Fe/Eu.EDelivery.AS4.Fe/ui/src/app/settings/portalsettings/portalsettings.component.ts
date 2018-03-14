import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Subscription } from 'rxjs/Subscription';

import { DialogService } from './../../common/dialog.service';
import { SettingsService } from './../settings.service';

@Component({
    selector: 'as4-portalsettings',
    templateUrl: 'portalsettings.component.html'
})
export class PortalSettingsComponent {
    public form: FormGroup;
    private _subscription: Subscription;
    constructor(private _formBuilder: FormBuilder, private _runtimeSettingsService: SettingsService, private _dialogService: DialogService) {
        this.form = this._formBuilder.group({
            authentication: this._formBuilder.group({
                connectionString: ['', Validators.required],
                provider: ['', Validators.required],
                jwtOptions: this._formBuilder.group({
                    issuer: ['', Validators.required],
                    audience: ['', Validators.required],
                    validFor: ['', Validators.required],
                    key: ['', Validators.required]
                })
            }),
            monitor: this._formBuilder.group({
                connectionString: ['', Validators.required],
                provider: ['', Validators.required]
            }),
            pmodes: this._formBuilder.group({
                receivingPmodeFolder: ['', Validators.required],
                sendingPmodeFolder: ['', Validators.required]
            }),
            settings: this._formBuilder.group({
                runtime: ['', Validators.required],
                settingsXml: ['', Validators.required],
                showStackTraceInExceptions: ['', Validators.required]
            }),
            port: ['', Validators.required],
            submitTool: this._formBuilder.group({
                payloadHttpAddress: ['', Validators.required],
                toHttpAddress: ['', Validators.required]
            })
        });

        this._subscription = this._runtimeSettingsService
            .getPortalSettings()
            .subscribe((result) => {
                this.form.patchValue(result);
                this.form.markAsPristine();
            });
    }
    public save() {
        this._runtimeSettingsService
            .savePortalSettings(this.form.value)
            .subscribe((result) => {
                this.form.markAsPristine();
                this._dialogService.message(`Because some data is cached in the browser, it's adviced to reload!`, 'Warning');
            });
    }
}
