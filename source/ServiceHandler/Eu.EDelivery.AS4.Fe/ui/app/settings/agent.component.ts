import { ReceiverComponent } from './receiver.component';
import { ActivatedRoute } from '@angular/router';
import { Component, Input, OnDestroy, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { NgForm, FormBuilder, FormGroup, FormArray } from '@angular/forms';

import { SettingsAgent } from '../api/SettingsAgent';
import { SettingsStore } from './settings.store';
import { SettingsService } from './settings.service';

@Component({
    selector: 'as4-agent-settings',
    templateUrl: './agent.component.html'
})
export class AgentSettingsComponent implements OnDestroy {
    public settings: SettingsAgent[];
    public collapsed: boolean = true;
    public currentAgent: SettingsAgent;

    public isDirty: boolean = false;
    public form: FormGroup;
    @Input() public title: string;
    @Input() public agent: string;

    private storeSubscr: Subscription;
    private _originalAgent: SettingsAgent | undefined;

    private initSettings() {
        // return this.currentAgent.receiver.setting.map(set => this.formBuilder.group({
        //     key: [''],
        //     value: ['']
        // }));

        return this.formBuilder.group({
            key: [''],
            value: ['']
        })
    }

    constructor(private settingsStore: SettingsStore, private settingsService: SettingsService, private activatedRoute: ActivatedRoute, private formBuilder: FormBuilder) {
        this.form = this.formBuilder.group({
            name: [''],
            receiver: this.formBuilder.group({
                type: [''],
                text: this.formBuilder.control(null),
                setting: this.formBuilder.array([

                ])
            }),
            transformer: [''],
            steps: [''],
            decorator: ['']
        });

        if (!!this.activatedRoute.snapshot.data['type']) {
            this.title = this.activatedRoute.snapshot.data['title'];
            this.collapsed = false;
        }
        this.storeSubscr = this.settingsStore.changes.subscribe(result => {
            let agent = this.agent;
            if (!!this.activatedRoute.snapshot.data['type']) {
                agent = this.activatedRoute.snapshot.data['type'];
            }
            console.log(agent);
            this.settings = result && result.Settings && result.Settings.agents && result.Settings.agents[agent];
        });
    }

    public addAgent() {
        this.currentAgent = new SettingsAgent();
        this.form.setValue(this.currentAgent);
    }

    public selectAgent(selectedAgent: string) {
        this.currentAgent = this.settings.find(agent => agent.name === selectedAgent);
        this.currentAgent.receiver.setting.forEach(recv => (<FormArray>(<FormGroup>this.form.controls['receiver']).controls['setting']).push(this.formBuilder.group({
            key: [''],
            value: ['']
        })));

        this.form.setValue(this.currentAgent);
        console.log(this.currentAgent);
        this.setPristine();
    }

    public save() {
        // this.settingsService
        //     .updateOrCreateSubmitAgent(this.currentAgent)
        //     .subscribe(() => this.form.control.markAsPristine());
    }

    public reset() {
        this.form.reset(this.currentAgent);
        // if (!!this._originalAgent) {
        //     var agent = this.settings.find(agent => agent.name === this._originalAgent.name);
        //     this.selectAgent(Object.assign(agent, this._originalAgent));
        //     this.setPristine();
        // }
    }
    public setPristine() {
        // setTimeout(() => {
        //     this.form.control.markAsPristine();
        // });
    }

    ngOnDestroy() {
        this.storeSubscr.unsubscribe();
    }
}
