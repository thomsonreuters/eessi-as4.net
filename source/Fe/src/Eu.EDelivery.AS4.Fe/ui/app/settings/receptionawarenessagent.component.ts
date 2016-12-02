import { FormGroup, FormBuilder } from '@angular/forms';
import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';

import { DialogService } from './../common/dialog.service';
import { RuntimeStore } from './runtime.store';
import { RuntimeService } from './runtime.service';
import { ItemType } from './../api/ItemType';
import { SettingsStore } from './settings.store';
import { SettingsAgent } from './../api/SettingsAgent';
import { SettingsService } from './settings.service';

@Component({
    selector: 'as4-receptionawareness-agent',
    templateUrl: './receptionawarenessagent.component.html'
})
export class ReceptionAwarenessAgentComponent implements OnDestroy {
    public form: FormGroup;
    public currentAgent: SettingsAgent;
    public transformers: Array<ItemType>;
    private _settingsStoreSubscr: Subscription;
    private _runtimeStoreSubscr: Subscription;
    constructor(private formBuilder: FormBuilder, private settingStore: SettingsStore, private settingsService: SettingsService,
        private runtimeService: RuntimeService, private runtimeStore: RuntimeStore, private dialogService: DialogService) {
        this.form = SettingsAgent.getForm(this.formBuilder, null);
        this._settingsStoreSubscr = this.settingStore
            .changes
            .filter(result => !!result && !!result.Settings.agents.receptionAwarenessAgent)
            .subscribe(result => {
                this.currentAgent = result.Settings.agents.receptionAwarenessAgent;
                this.form = SettingsAgent.getForm(this.formBuilder, this.currentAgent);
            });
        this._runtimeStoreSubscr = this.runtimeStore
            .changes
            .filter(result => !!result && !!result.transformers)
            .subscribe(result => {
                this.transformers = result.transformers;
            });
    }
    save() {
        if (!this.form.valid) {
            this.dialogService.incorrectForm();
            return;
        }

        this.settingsService
            .updateAgent(this.form.value, this.currentAgent.name, 'receptionAwarenessAgent')
            .subscribe(result => {
                if (result) {
                    console.log('HALLO TEST');
                    this.form.markAsPristine();
                    this.currentAgent = <SettingsAgent>this.form.value;
                }
            });
    }
    reset() {
        this.form = SettingsAgent.getForm(this.formBuilder, this.currentAgent);
    }
    ngOnDestroy() {
        this._settingsStoreSubscr.unsubscribe();
        this._runtimeStoreSubscr.unsubscribe();
    }
}
