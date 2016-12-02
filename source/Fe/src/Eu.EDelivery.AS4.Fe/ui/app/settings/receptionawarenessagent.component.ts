import { SettingsAgents } from './../api/SettingsAgents';
import { FormGroup, FormBuilder } from '@angular/forms';
import { Component, OnDestroy } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { Observable } from 'rxjs/Observable';

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
    public currentAgent: SettingsAgent = new SettingsAgent();
    public transformers: Array<ItemType>;
    private isNew: boolean = true;
    private _settingsStoreSubscr: Subscription;
    private _runtimeStoreSubscr: Subscription;
    constructor(private formBuilder: FormBuilder, private settingStore: SettingsStore, private settingsService: SettingsService,
        private runtimeService: RuntimeService, private runtimeStore: RuntimeStore, private dialogService: DialogService) {
        this.form = SettingsAgent.getForm(this.formBuilder, null);
        this._settingsStoreSubscr = this.settingStore
            .changes
            .filter(result => !!result && !!result.Settings && !!result.Settings.agents)
            .map(result => result.Settings.agents.receptionAwarenessAgent)
            .subscribe(result => {
                this.isNew = !!!result;
                this.currentAgent = !!!result ? new SettingsAgent() : result;
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

        let obs: Observable<boolean>;
        if (this.isNew) obs = this.settingsService.createAgent(this.form.value, SettingsAgents.FIELD_receptionAwarenessAgent);
        else obs = this.settingsService.updateAgent(this.form.value, this.currentAgent.name, SettingsAgents.FIELD_receptionAwarenessAgent);
        obs.subscribe(result => {
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
    delete() {
        if (!this.dialogService.deleteConfirm('agent')) return;
        this.settingsService.deleteAgent(this.currentAgent, SettingsAgents.FIELD_receptionAwarenessAgent);
    }
    ngOnDestroy() {
        this._settingsStoreSubscr.unsubscribe();
        this._runtimeStoreSubscr.unsubscribe();
    }
}
