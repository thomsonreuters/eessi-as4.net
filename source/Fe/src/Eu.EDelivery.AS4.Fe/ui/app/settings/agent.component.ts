import { Observer } from 'rxjs/Observer';
import { removeNgStyles } from '@angularclass/hmr';
import { ActivatedRoute } from '@angular/router';
import { Component, Input, OnDestroy, ViewChild, ElementRef, NgZone } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { NgForm, FormBuilder, FormGroup, FormArray } from '@angular/forms';

import { RuntimeStore } from './runtime.store';
import { Setting } from './../api/Setting';
import { Steps } from './../api/Steps';
import { Step } from './../api/Step';
import { Transformer } from './../api/Transformer';
import { Receiver } from './../api/Receiver';
import { ReceiverComponent } from './receiver.component';
import { SettingsAgent, OriginalSettingsAgent } from '../api/SettingsAgent';
import { SettingsService } from './settings.service';
import { SettingsStore } from './settings.store';
import { DialogService } from './../common/dialog.service';
import { ItemType } from './../api/ItemType';
import { Property } from './../api/Property';

@Component({
    selector: 'as4-agent-settings',
    templateUrl: './agent.component.html'
})
export class AgentSettingsComponent implements OnDestroy {
    public settings: SettingsAgent[] = new Array<SettingsAgent>();
    public collapsed: boolean = true;

    public get currentAgent(): SettingsAgent {
        return this._currentAgent;
    }
    public set currentAgent(agent: SettingsAgent) {
        this._currentAgent = agent;
    }
    public transformers: ItemType[];
    public isNewMode: boolean = false;

    public form: FormGroup;
    @Input() public title: string;
    @Input() public agent: string;
    @ViewChild('dropdown') public dropdown: ElementRef;

    private _currentAgent: SettingsAgent;
    private _settingsStoreSubscription: Subscription;
    private _runtimeStoreSubscription: Subscription;

    constructor(private settingsStore: SettingsStore, private settingsService: SettingsService, private activatedRoute: ActivatedRoute, private formBuilder: FormBuilder,
        private runtimeStore: RuntimeStore, private dialogService: DialogService, private ngZone: NgZone) {
        this.form = SettingsAgent.getForm(this.formBuilder, null);
        if (!!this.activatedRoute.snapshot.data['type']) {
            this.title = `${this.activatedRoute.snapshot.data['title']} agent`;
            this.collapsed = false;
            this.agent = this.activatedRoute.snapshot.data['type'];
        }

        this._runtimeStoreSubscription = this.runtimeStore
            .changes
            .filter(x => x != null)
            .subscribe(result => {
                this.transformers = result.transformers;
            });

        this._settingsStoreSubscription = this.settingsStore
            .changes
            .filter(result => !!result && !!result.Settings && !!result.Settings.agents)
            .map(result => result.Settings.agents[this.agent])
            .distinctUntilChanged()
            .subscribe(result => {
                this.settings = result;
                this.currentAgent = this.settings.find(agt => agt.name === this.form.value.name);
                if (!!this.currentAgent) {
                    this.form = SettingsAgent.getForm(this.formBuilder, this.currentAgent);
                }
            });
    }
    public addAgent() {
        let newName = this.dialogService.prompt('Please enter a new for the agent');
        if (!!!newName) return;
        let newAgent = new SettingsAgent();
        newAgent.name = newName;
        if (!this.selectAgent(newAgent.name)) return;
        this.settings.push(newAgent);
        this.currentAgent = newAgent;
        this.form.patchValue({ [SettingsAgent.name]: newName });
        this.isNewMode = true;
        this.form = SettingsAgent.getForm(this.formBuilder, this.currentAgent);
    }
    public selectAgent(selectedAgent: string = null, $event: Event = null): boolean {
        if (this.form.dirty) {
            if (this.dialogService.confirm('There are unsaved changes, are you sure you want to cancel the changes?')) {
                if (this.isNewMode) this.settings = this.settings.filter(agent => agent !== this.currentAgent);
                else this.reset();
            }
            else return false;
        }
        this.isNewMode = false;
        this.currentAgent = this.settings.find(agent => agent.name === selectedAgent);
        this.form = SettingsAgent.getForm(this.formBuilder, this.currentAgent);
        return true;
    }
    public save() {
        if (!this.form.valid) {
            this.dialogService.message('Input is not valid, please correct the invalid fields');
            return;
        }
        let obs;
        if (!this.isNewMode) obs = this.settingsService.updateAgent(this.form.value, this.currentAgent.name, this.agent);
        else obs = this.settingsService.createAgent(this.form.value, this.agent);
        obs.subscribe(result => {
            if (result) {
                this.isNewMode = false;
                this.form.markAsPristine();
            }
        });
    }
    public reset() {
        if (this.isNewMode) {
            this.settings = this.settings.filter(agent => agent !== this.currentAgent);
        }
        this.form = SettingsAgent.getForm(this.formBuilder, this.currentAgent);
    }
    public rename() {
        let name = this.dialogService.prompt('Enter new name');
        if (!!this.currentAgent && !!name) {
            this.form.patchValue({ [SettingsAgent.FIELD_name]: name });
            this.form.markAsDirty();
        }
    }
    public delete() {
        if (this.dialogService.confirm('Are you sure you want to delete the agent')) {
            if (this.isNewMode) {
                this.settings = this.settings.filter(agent => agent.name !== this.currentAgent.name);
                this.selectAgent();
                return;
            }

            this.settingsService.deleteAgent(this.currentAgent, this.agent);
        }
    }

    ngOnDestroy() {
        this._settingsStoreSubscription.unsubscribe();
        this._runtimeStoreSubscription.unsubscribe();
    }
}
