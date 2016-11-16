import { Component, Input } from '@angular/core';

import { SettingsAgent } from '../api/SettingsAgent';

@Component({
    selector: 'as4-agent-settings',
    template: `
        <as4-receiver [settings]="settings?.receiver"></as4-receiver>
        <p>Transformer: {{settings?.transformer?.type}}</p>
        <as4-decorator [settings]="settings?.decorator"></as4-decorator>
        <p>Name: {{settings?.name}}</p>
    `
})
export class AgentSettingsComponent {
    @Input() settings: SettingsAgent;
    @Input() title: string;
}
