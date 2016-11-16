import { Component, Input } from '@angular/core';

import { Step } from './../api/Step';

@Component({
    selector: 'as4-step-settings',
    template: `
    `
})
export class BaseSettingsComponent {
    @Input() settings: Step;
}

