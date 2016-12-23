import { Component, OnInit, Input, ChangeDetectionStrategy } from '@angular/core';

import { ItemType } from './../../api/ItemType';
import { Settings } from './../../api/Settings';

@Component({
    selector: 'as4-Settings',
    templateUrl: './Settings.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class SettingsComponent implements OnInit {
    @Input() properties: ItemType[];
    constructor() {
    }

    ngOnInit() {
    }
}
