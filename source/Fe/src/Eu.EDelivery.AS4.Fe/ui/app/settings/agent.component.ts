import { ActivatedRoute } from '@angular/router';
import { Component, Input, OnDestroy, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { NgForm, FormBuilder, FormGroup, FormArray } from '@angular/forms';

import { RuntimeStore } from './runtime.store';
import { ItemType } from './runtime.service';
import { Setting } from './../api/Setting';
import { Decorator } from './../api/Decorator';
import { Steps } from './../api/Steps';
import { Step } from './../api/Step';
import { Transformer } from './../api/Transformer';
import { Receiver } from './../api/Receiver';
import { ReceiverComponent } from './receiver.component';
import { SettingsAgent } from '../api/SettingsAgent';
import { SettingsService } from './settings.service';
import { SettingsStore } from './settings.store';

@Component({
    selector: 'as4-agent-settings',
    templateUrl: './agent.component.html'
})
export class AgentSettingsComponent implements OnDestroy {
    public settings: SettingsAgent[];
    public collapsed: boolean = true;
    public currentAgent: SettingsAgent;
    public transformers: ItemType[];

    public isDirty: boolean = false;
    public form: FormGroup;
    @Input() public title: string;
    @Input() public agent: string;

    private _settingsStoreSubscription: Subscription;
    private _runtimeStoreSubscription: Subscription;
    private _originalAgent: SettingsAgent | undefined;

    constructor(private settingsStore: SettingsStore, private settingsService: SettingsService, private activatedRoute: ActivatedRoute, private formBuilder: FormBuilder,
        private runtimeStore: RuntimeStore) {
        this._runtimeStoreSubscription = this.runtimeStore.changes
            .filter(x => x != null)
            .subscribe(result => {
                this.transformers = result.transformers;
            });
        this.form = SettingsAgent.getForm(this.formBuilder, null);

        if (!!this.activatedRoute.snapshot.data['type']) {
            this.title = `${this.activatedRoute.snapshot.data['title']} agent`;
            this.collapsed = false;
        }
        this._settingsStoreSubscription = this.settingsStore.changes.subscribe(result => {
            let agent = this.agent;
            if (!!this.activatedRoute.snapshot.data['type']) {
                agent = this.activatedRoute.snapshot.data['type'];
            }
            this.settings = result && result.Settings && result.Settings.agents && result.Settings.agents[agent];
        });
    }

    public addAgent() {
        this.currentAgent = new SettingsAgent();
        this.form.setValue(this.currentAgent);
    }

    public selectAgent(selectedAgent: string) {
        this.currentAgent = this.settings.find(agent => agent.name === selectedAgent);
        this.form = SettingsAgent.getForm(this.formBuilder, this.currentAgent);
    }

    public save() {
        this.settingsService
            .updateOrCreateSubmitAgent(this.form.value)
            .subscribe(result => {
                if (result) {
                    alert('Saved');
                    this.form.markAsPristine();
                }
            });
    }

    public reset() {
        this.form = SettingsAgent.getForm(this.formBuilder, this.currentAgent);
    }

    ngOnDestroy() {
        this._settingsStoreSubscription.unsubscribe();
        this._runtimeStoreSubscription.unsubscribe();
    }
}
