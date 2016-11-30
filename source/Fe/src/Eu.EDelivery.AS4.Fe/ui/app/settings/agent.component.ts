import { removeNgStyles } from '@angularclass/hmr';
import { ActivatedRoute } from '@angular/router';
import { Component, Input, OnDestroy, ViewChild, ElementRef } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { NgForm, FormBuilder, FormGroup, FormArray } from '@angular/forms';

import { RuntimeStore } from './runtime.store';
import { Setting } from './../api/Setting';
import { Steps } from './../api/Steps';
import { Step } from './../api/Step';
import { Transformer } from './../api/Transformer';
import { Receiver } from './../api/Receiver';
import { ReceiverComponent } from './receiver.component';
import { SettingsAgent } from '../api/SettingsAgent';
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
    public settings: SettingsAgent[];
    public collapsed: boolean = true;

    public currentAgent: SettingsAgent;
    public transformers: ItemType[];
    public isNewMode: boolean = false;

    public form: FormGroup;
    @Input() public title: string;
    @Input() public agent: string;
    @ViewChild('dropdown') private dropdown: ElementRef;

    private _settingsStoreSubscription: Subscription;
    private _runtimeStoreSubscription: Subscription;

    constructor(private settingsStore: SettingsStore, private settingsService: SettingsService, private activatedRoute: ActivatedRoute, private formBuilder: FormBuilder,
        private runtimeStore: RuntimeStore, private dialogService: DialogService) {
        this._runtimeStoreSubscription = this.runtimeStore.changes
            .filter(x => x != null)
            .subscribe(result => {
                this.transformers = result.transformers;
            });
        this._settingsStoreSubscription = this.settingsStore.changes.subscribe(result => {
            if (!!this.activatedRoute.snapshot.data['type']) {
                this.agent = this.activatedRoute.snapshot.data['type'];
            }
            this.settings = result && result.Settings && result.Settings.agents && result.Settings.agents[this.agent];
            if (!!!this.settings) this.settings = new Array<SettingsAgent>();
            this.form = SettingsAgent.getForm(this.formBuilder, null);
        });
        if (!!this.activatedRoute.snapshot.data['type']) {
            this.title = `${this.activatedRoute.snapshot.data['title']} agent`;
            this.collapsed = false;
        }
    }
    public addAgent() {
        let newName = this.dialogService.prompt('Please enter a new for the agent');
        if (!!!newName) return;
        let newAgent = new SettingsAgent();
        newAgent.name = newName;
        if (!this.selectAgent(newAgent.name)) return;
        this.settings.push(this.currentAgent);
        this.currentAgent = newAgent;
        this.isNewMode = true;
    }
    public selectAgent(selectedAgent: string = null, $event: Event = null): boolean {
        if (this.form.dirty) {
            if (this.dialogService.confirm('There are unsaved changes, are you sure you want to cancel the changes?')) {
                if (this.isNewMode) this.settings = this.settings.filter(agent => agent !== this.currentAgent);
                else this.form.reset();
            }
            else {
                this.dropdown.nativeElement.value = this.currentAgent.name;
                return false;
            }
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

        this.settingsService
            .updateOrCreateAgent(this.form.value, this.agent)
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
    public rename() {
        let name = this.dialogService.prompt('Enter new name');
        // TODO: find a better way to be able to do a revert, since we're directly changing the currentAgent name value
        if (!!this.currentAgent) {
            this.currentAgent.name = name;
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
